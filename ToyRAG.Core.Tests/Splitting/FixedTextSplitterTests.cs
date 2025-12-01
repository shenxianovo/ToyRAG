using ToyRAG.Core.Loading;
using ToyRAG.Core.Splitting;

namespace ToyRAG.Core.Tests.Splitting
{
    [TestClass]
    public sealed class FixedTextSplitterTests
    {
        [TestMethod]
        public void Split_ShouldSplitDocumentCorrectly()
        {
            var testDirectoryPath = @"E:\Code\Local\Learning\C#\ToyRAG\data\docs\standard\serialization";
            var loader = new MarkdownLoader();
            var docs = loader.Load(testDirectoryPath);
            var doc = docs.ElementAt(7);

            var splitter = new FixedTextSplitter();
            var result = splitter.Split(doc);


            Assert.AreEqual(1000, result.ElementAt(0).Content!.Length);
        }
    }
}
