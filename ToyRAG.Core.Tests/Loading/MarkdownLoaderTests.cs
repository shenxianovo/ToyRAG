using System.Threading;
using ToyRAG.Core.Loading;

namespace ToyRAG.Core.Tests.Loading
{
    [TestClass]
    public sealed class MarkdownLoaderTests
    {
        [TestMethod]
        public void Load_ShouldParseYamlAndBodyCorrectly()
        {
            var testDirectoryPath = @"E:\Code\Local\Learning\C#\ToyRAG\data\docs\standard\serialization";
            var loader = new MarkdownLoader();
            var docs = loader.Load(testDirectoryPath);

            var doc = docs.ElementAt(7);

            Assert.IsNotNull(doc.MetaData);
            Assert.IsTrue(doc.MetaData.ContainsKey("title"));
            Assert.AreEqual("<dateTimeSerialization> Element", doc.MetaData["title"]);

            Assert.Contains("When this property is set to **Local**", doc.Body!);

            string expectedFullPath = Path.GetFullPath("E:\\Code\\Local\\Learning\\C#\\ToyRAG\\data\\docs\\standard\\serialization\\datetimeserialization-element.md");
            string actualPath = Path.GetFullPath(doc.Source!);
            Assert.AreEqual(expectedFullPath, actualPath);
        }
    }
}
