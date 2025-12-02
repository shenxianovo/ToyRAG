using ToyRAG.Core.Entities;

namespace ToyRAG.Core.Embeddings
{
    public interface IEmbeddingGenerator
    {
        /// <summary>
        /// 为文本块生成向量，并填充到 TextChunk.Embedding 属性中
        /// </summary>
        Task GenerateAsync(IEnumerable<TextChunk> chunks);
    }
}
