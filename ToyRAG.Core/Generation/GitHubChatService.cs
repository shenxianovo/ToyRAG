
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

namespace ToyRAG.Core.Generation
{
    public class GitHubChatService : IChatService
    {
        private readonly AIAgent _agent;
        private readonly AgentThread _agentThread;

        public GitHubChatService(
            string gitHubToken,
            string model = "gpt-4o-mini",
            Func<AIAgent, FunctionInvocationContext, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>>, CancellationToken, ValueTask<object?>> middleware = null
            )
        {
            OpenAIClientOptions openAIClientOptions = new()
            {
                Endpoint = new Uri("https://models.github.ai/inference")
            };
            OpenAIClient client = new(new ApiKeyCredential(gitHubToken), openAIClientOptions);
            _agent = client
                .GetChatClient(model)
                .CreateAIAgent(
                    name: "Assistant",
                    instructions: """
                    你是一个基于检索增强生成（RAG）的问答助手。
                    你必须严格依据提供的【上下文】回答问题。

                    规则：
                    1. 只能使用上下文中的信息进行回答，不允许使用外部常识或自行推断。
                    2. 如果上下文中没有足够信息，必须明确回答“根据提供的文档无法确定”，不要编造答案。
                    3. 回答应简洁、准确，优先给出结论，其次给出依据。
                    4. 如果问题包含多个子问题，请逐条回答。
                    5. 不要提及“上下文”“文档编号”等内部实现细节。
                    """
                );
            if (middleware is not null)
            {
                _agent = _agent.AsBuilder().Use(middleware).Build();
            }
                
            _agentThread = _agent.GetNewThread();
        }

        public async Task<string> GenerateAsync(string message) => (await _agent.RunAsync(message, _agentThread)).ToString();
    }
}
