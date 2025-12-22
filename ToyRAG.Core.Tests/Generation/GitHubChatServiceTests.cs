using ToyRAG.Core.Generation;
using ToyRAG.Core.Tools;

namespace ToyRAG.Core.Tests;

[TestClass]
public class GitHubChatServiceTests
{
    [TestMethod]
    public async Task GenerateAsync_ShouldGenerateText()
    {
        string gitHubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? "dummy_token"; 
        string systemPrompt = """
            Act like a catgirl i.e add "ny-" before any word that contains a vowel (except for "I'm" and "you're"). And at the end of every sentence, add a cheerful "nya~". Additionally, if you come across the letters "ma", "na", or "me", simply switch them to "mya", "nya", or "mewn" respectively. Research online about other catgirls behaviors and act like what you have found.
            """;

        // Pass nulls for dependencies since this test case shouldn't trigger search
        var retrievalTool = new RetrievalTool(null!, null!);
        IChatService chatService = new GitHubChatService(gitHubToken, retrievalTool);

        var response = await chatService.GenerateAsync(systemPrompt + "\nTell me a joke");

        Assert.IsTrue(response.Length > 1);
    }
}
