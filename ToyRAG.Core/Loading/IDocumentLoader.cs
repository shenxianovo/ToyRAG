using ToyRAG.Core.Entities;

namespace ToyRAG.Core.Loading
{
    public interface IDocumentLoader
    {
        /// <summary>
        /// 加载指定目录下的所有文档
        /// </summary>
        /// <param name="directoryPath">目录路径</param>
        /// <returns>文档列表</returns>
        List<Document> Load(string directoryPath);
    }
}
