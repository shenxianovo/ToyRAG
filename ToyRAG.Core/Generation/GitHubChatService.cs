
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace ToyRAG.Core.Generation
{
    public class GitHubChatService : IChatService
    {
        private readonly ChatClient _client;

        private const string GitHubEndpoint = "https://models.inference.ai.azure.com";

        public GitHubChatService(string githubToken, string model = "gpt-4o")
        {
            // 使用 GitHub Token 创建认证信息
            var credential = new ApiKeyCredential(githubToken);

            // 创建 OpenAIClient，但指向 GitHub 的端点
            var openAiClient = new OpenAIClient(credential, new OpenAIClientOptions
            {
                Endpoint = new Uri(GitHubEndpoint)
            });

            _client = openAiClient.GetChatClient(model);
        }

        public async Task<string> GenerateAsync(string systemPrompt, string userPrompt)
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userPrompt)
            };

            ChatCompletion completion = await _client.CompleteChatAsync(messages);

            return completion.Content[0].Text;
        }
    }
}
