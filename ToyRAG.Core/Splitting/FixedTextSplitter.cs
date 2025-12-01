using ToyRAG.Core.Entities;

namespace ToyRAG.Core.Splitting
{
    public class FixedTextSplitter : ITextSplitter
    {
        public int ChunkSize { get; set; } = 1000;
        public int Overlap = 100;
        public IEnumerable<TextChunk> Split(Document document)
        {
            if (document.Body is null)
                yield break;

            string text = document.Body;
            int length = text.Length;

            int index = 0;
            int chunkId = 0;

            while (index < length)
            {
                int remaining = length - index;
                int size = Math.Min(ChunkSize, remaining);

                string chunkText = text.Substring(index, size);

                yield return new TextChunk
                {
                    Id = chunkId.ToString(),
                    Content = chunkText,
                    DocumentId = document.Id,
                    Source = document.Source,
                    MetaData = document.MetaData
                };

                chunkId++;

                index += ChunkSize - Overlap;
                if (index < 0)
                    break;
            }

        }
    }
}
