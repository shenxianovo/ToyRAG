using ToyRAG.Core.Entities;

namespace ToyRAG.Core.Storage
{
    public interface IVectorStore
    {
        /// <summary>
        /// 保存文本块
        /// </summary>
        Task SaveAsync(IEnumerable<TextChunk> chunks);

        /// <summary>
        /// 检索最相似的文本块
        /// </summary>
        /// <param name="queryEmbedding">查询向量</param>
        /// <param name="limit">返回数量限制</param>
        /// <returns>按相似度排序的文本块列表 (包含相似度分数)</returns>
        Task<IEnumerable<(TextChunk Chunk, float Score)>> SearchAsync(float[] queryEmbedding, int limit = 5);

        /// <summary>
        /// 检查指定源文件的文档是否已存在
        /// </summary>
        Task<bool> IsDocumentIndexedAsync(string source);
    }
}
