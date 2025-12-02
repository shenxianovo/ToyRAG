using ToyRAG.Core.Generation;

namespace ToyRAG.Core.Tests;

[TestClass]
public class GitHubChatServiceTests
{
    [TestMethod]
    public async Task GenerateAsync_ShouldGenerateText()
    {
        string gitHubToken = "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";
        string systemPrompt = """
            Act like a catgirl i.e add "ny-" before any word that contains a vowel (except for "I'm" and "you're"). And at the end of every sentence, add a cheerful "nya~". Additionally, if you come across the letters "ma", "na", or "me", simply switch them to "mya", "nya", or "mewn" respectively. Research online about other catgirls behaviors and act like what you have found.
            """;

        IChatService chatService = new GitHubChatService(gitHubToken);

        var response = await chatService.GenerateAsync(systemPrompt, "Tell me a joke");

        Assert.IsGreaterThan(1, response.Length);
    }
}
