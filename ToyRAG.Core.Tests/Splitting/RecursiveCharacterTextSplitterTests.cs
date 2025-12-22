using ToyRAG.Core.Entities;
using ToyRAG.Core.Splitting;

namespace ToyRAG.Core.Tests.Splitting
{
    [TestClass]
    public sealed class RecursiveCharacterTextSplitterTests
    {
        [TestMethod]
        public void Split_ShouldSplitByNewlines()
        {
            var text = "Paragraph 1\n\nParagraph 2\n\nParagraph 3";
            var doc = new Document { Body = text, Id = "doc1", Source = "test" };

            var splitter = new RecursiveCharacterTextSplitter
            {
                ChunkSize = 15, // Small size to force split
                Separators = ["\n\n"]
            };

            var chunks = splitter.Split(doc).ToList();

            Assert.AreEqual(3, chunks.Count);
            Assert.AreEqual("Paragraph 1", chunks[0].Content);
            Assert.AreEqual("Paragraph 2", chunks[1].Content);
            Assert.AreEqual("Paragraph 3", chunks[2].Content);
        }

        [TestMethod]
        public void Split_ShouldRespectChunkSize()
        {
            // A long string without separators
            var text = new string('a', 100);
            var doc = new Document { Body = text, Id = "doc1", Source = "test" };

            var splitter = new RecursiveCharacterTextSplitter
            {
                ChunkSize = 20,
                ChunkOverlap = 0
            };

            var chunks = splitter.Split(doc).ToList();

            Assert.AreEqual(5, chunks.Count);
            Assert.IsTrue(chunks.All(c => c.Content.Length <= 20));
        }
    }
}