using ToyRAG.Core.Embeddings;
using ToyRAG.Core.Entities;

namespace ToyRAG.Core.Tests;

[TestClass]
public class MiniLMEmbeddingGeneratorTests
{
    [TestMethod]
    public async Task GenerateAsync_ShpuldGenerateEmbeddingsCorrectly()
    {
        var generator = new MiniLMEmbeddingGenerator(
            @"E:\Code\Local\Learning\C#\ToyRAG\models\all-MiniLM-L12-v2.onnx",
            @"E:\Code\Local\Learning\C#\ToyRAG\models\vocab.txt"
        );

        var chunks = new List<TextChunk>
        {
            new TextChunk { Content = "This is a test sentence." },
            new TextChunk { Content = "Another example text for embedding generation." }
        };

        await generator.GenerateAsync(chunks);

        foreach (var chunk in chunks)
        {
            Console.WriteLine($"Embedding for chunk '{chunk.Content}': {string.Join(", ", chunk.Embedding!.Take(5))}...");
        }
    }
}
