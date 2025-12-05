using ToyRAG.Core.Entities;
using ToyRAG.Core.Storage;

namespace ToyRAG.Core.Tests;

[TestClass]
public class InMemoryVectorStoreTests
{
    [TestMethod]
    public async Task SaveAsync_ShouldStoreChunksWithEmbeddings()
    {
        var vectorStore = new InMemoryVectorStore();
        var chunks = new List<TextChunk>
            {
                new TextChunk { Content = "Chunk 1", Embedding = new float[] { 0.1f, 0.2f, 0.3f } },
                new TextChunk { Content = "Chunk 2", Embedding = null },
                new TextChunk { Content = "Chunk 3", Embedding = new float[] { 0.4f, 0.5f, 0.6f } }
            };

        await vectorStore.SaveAsync(chunks);

        var savedChunks = await vectorStore.SearchAsync(new float[] { 0, 0, 0 }, 10);
        Assert.AreEqual(2, savedChunks.Count());
    }

    public async Task SearchAsync_ShouldReturnTopResults()
    {
        var vectorStore = new InMemoryVectorStore();
        var chunks = new List<TextChunk>
            {
                new TextChunk { Content = "Chunk 1", Embedding = new float[] { 1, 0, 0 } },
                new TextChunk { Content = "Chunk 2", Embedding = new float[] { 0, 1, 0 } },
                new TextChunk { Content = "Chunk 3", Embedding = new float[] { 0, 0, 1 } }
            };
        await vectorStore.SaveAsync(chunks);

        var queryEmbedding = new float[] { 1, 0, 0 };

        var results = await vectorStore.SearchAsync(queryEmbedding, 2);

        Assert.AreEqual(2, results.Count());
        Assert.AreEqual("Chunk 1", results.First().Chunk.Content);
        Assert.IsGreaterThan(results.First().Score, results.Last().Score);
    }
}
