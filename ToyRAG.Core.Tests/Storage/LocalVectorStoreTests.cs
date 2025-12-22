using System.Data.SQLite;
using ToyRAG.Core.Entities;
using ToyRAG.Core.Storage;

namespace ToyRAG.Core.Tests.Storage
{
    [TestClass]
    public class LocalVectorStoreTests
    {
        private string _dbPath;

        [TestInitialize]
        public void Setup()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), $"test_vectors_{Guid.NewGuid()}.db");
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_dbPath))
            {
                // Ensure connection is closed before deleting (SQLite requirement)
                GC.Collect();
                GC.WaitForPendingFinalizers();
                try { File.Delete(_dbPath); } catch { }
            }
        }

        [TestMethod]
        public async Task IsDocumentIndexedAsync_ShouldWorkWithSQLite()
        {
            var store = new LocalVectorStrore(_dbPath);
            var chunks = new List<TextChunk>
            {
                new TextChunk 
                { 
                    Id = "1",
                    Content = "Content", 
                    Source = "doc1.md", 
                    Embedding = new float[] { 0.1f, 0.2f },
                    MetaData = new Dictionary<string, object> { { "key", "value" } }
                }
            };

            await store.SaveAsync(chunks);

            bool exists = await store.IsDocumentIndexedAsync("doc1.md");
            bool notExists = await store.IsDocumentIndexedAsync("doc2.md");

            Assert.IsTrue(exists, "Should find doc1.md");
            Assert.IsFalse(notExists, "Should not find doc2.md");
        }
    }
}