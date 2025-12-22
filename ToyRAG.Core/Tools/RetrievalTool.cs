using System.ComponentModel;
using System.Text;
using ToyRAG.Core.Embeddings;
using ToyRAG.Core.Storage;

namespace ToyRAG.Core.Tools
{
    public class RetrievalTool(IEmbeddingGenerator embeddingGenerator, IVectorStore vectorStore)
    {
        [Description("Search the knowledge base for relevant information.")]
        public async Task<string> SearchAsync([Description("The search query")] string query)
        {
            var queryEmbedding = embeddingGenerator.GenerateEmbeddings(query.ToLowerInvariant());
            var results = await vectorStore.SearchAsync(queryEmbedding);

            if (!results.Any())
            {
                return "No relevant information found.";
            }

            StringBuilder sb = new();
            foreach (var (chunk, score) in results)
            {
                sb.AppendLine($"--- Source: {chunk.Source} (Score: {score:F2}) ---");
                sb.AppendLine(chunk.Content);
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
