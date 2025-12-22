using Microsoft.Agents.AI;
using OpenAI;
using System.ClientModel;
using ToyRAG.Core.Tools;

namespace ToyRAG.Core.Generation
{
    public class GitHubChatService : IChatService
    {
        private readonly AIAgent _agent;
        private readonly AgentThread _agentThread;
        private readonly RetrievalTool _retrievalTool;

        public GitHubChatService(
            string gitHubToken,
            RetrievalTool retrievalTool,
            string model = "gpt-4o-mini"
            )
        {
            _retrievalTool = retrievalTool;

            OpenAIClientOptions openAIClientOptions = new()
            {
                Endpoint = new Uri("https://models.github.ai/inference")
            };
            OpenAIClient client = new(new ApiKeyCredential(gitHubToken), openAIClientOptions);
            
            // Use basic CreateAIAgent without tools middleware
            _agent = client.GetChatClient(model)
                .CreateAIAgent(
                    name: "Assistant",
                    instructions: """
                    你是一个基于检索增强生成（RAG）的问答助手。

                    工作流程：
                    1. 用户提问。
                    2. 如果你不知道，请【搜索外部知识】，此时，你**仅**输出一行以 "SEARCH:" 开头的指令：
                       SEARCH: 你的搜索关键词
                    3. 如果你不需要搜索（例如通用闲聊），请直接回答。
                    4. 当你收到搜索结果后，根据结果回答用户问题。
                    
                    规则：
                    - 搜索关键词应精简且核心。
                    - 严格依据搜索结果回答。
                    - 如果搜索结果不相关，请告知无法回答。
                    """
                );
                
            _agentThread = _agent.GetNewThread();
        }

        public async Task<string> GenerateAsync(string message)
        {
            // 1. Initial Call
            var response = await _agent.RunAsync(message, _agentThread);
            var content = response.ToString()!;

            // 2. Check for Tool Call (Manual Pattern Matching)
            // This implements the "LLM decides to call retrieval" logic requested.
            if (content.Trim().StartsWith("SEARCH:", StringComparison.OrdinalIgnoreCase))
            {
                var query = content.Split(':', 2)[1].Trim();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n[Agent Request] 正在搜索: {query}..."); 
                Console.ResetColor();
                
                var searchResult = await _retrievalTool.SearchAsync(query);
                
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("\n[搜索结果]");
                Console.WriteLine(searchResult);
                Console.ResetColor();
                
                // 3. Feed back results to the Agent
                var followUp = $"""
                [System: Search Results]
                {searchResult}
                
                请根据以上搜索结果回答用户最初的问题。
                """;

                response = await _agent.RunAsync(followUp, _agentThread);
                content = response.ToString()!;
            }

            return content;
        }
    }
}
