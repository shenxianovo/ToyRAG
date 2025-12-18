using ToyRAG.Core.Entities;

namespace ToyRAG.Core.Storage
{
    public class InMemoryVectorStore : IVectorStore
    {
        private List<TextChunk> _store = [];

        public Task<bool> IsDocumentIndexedAsync(string source)
        {
            throw new NotImplementedException();
        }

        public Task SaveAsync(IEnumerable<TextChunk> chunks)
        {
            foreach (var chunk in chunks)
            {
                if (chunk.Embedding is not null)
                {
                    _store.Add(chunk);
                }
            }
            return Task.CompletedTask;
        }

        public Task<IEnumerable<(TextChunk Chunk, float Score)>> SearchAsync(float[] queryEmbedding, int limit = 5)
        {
            var results = new List<(TextChunk Chunk, float Score)>();

            foreach (var chunk in _store)
            {
                if (chunk.Embedding == null || chunk.Embedding.Length != queryEmbedding.Length)
                    continue;

                float dotProduct = 0;
                for (int i = 0; i < queryEmbedding.Length; i++)
                {
                    dotProduct += chunk.Embedding[i] * queryEmbedding[i];
                }

                results.Add((chunk, dotProduct));
            }

            var topResults = results
                .OrderByDescending(x => x.Score)
                .Take(limit);

            return Task.FromResult(topResults);
        }
    }
}
