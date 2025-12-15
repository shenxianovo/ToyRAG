
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace ToyRAG.Core.Generation
{
    public class GitHubChatService : IChatService
    {
        private readonly AIAgent _agent;
        private readonly AgentThread _agentThread;

        private const string GitHubEndpoint = "https://models.github.ai/inference";

        public GitHubChatService(string gitHubToken, string model = "gpt-4o")
        {
            OpenAIClientOptions openAIClientOptions = new()
            {
                Endpoint = new Uri("https://models.github.ai/inference")
            };
            OpenAIClient client = new(new ApiKeyCredential(gitHubToken), openAIClientOptions);
            _agent = client
                .GetChatClient("gpt-4o-mini")
                .CreateAIAgent(
                    instructions: "你是一个智能助手。请根据提供的参考资料回答用户的问题，回答时请引用数据来源。如果参考资料中没有答案，请直接说不知道。",
                    name: "Assistant"
                );
            _agentThread = _agent.GetNewThread();
        }

        public async Task<string> GenerateAsync(string message) => (await _agent.RunAsync(message, _agentThread)).ToString();
    }
}
