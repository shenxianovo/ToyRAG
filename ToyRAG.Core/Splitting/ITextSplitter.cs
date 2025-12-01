using ToyRAG.Core.Entities;

namespace ToyRAG.Core.Splitting
{
    public interface ITextSplitter
    {
        /// <summary>
        /// 将文档切分为多个文本块
        /// </summary>
        IEnumerable<TextChunk> Split(Document document);
    }
}
