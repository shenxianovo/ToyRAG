using System.Text;
using ToyRAG.Core.Embeddings;
using ToyRAG.Core.Generation;
using ToyRAG.Core.Loading;
using ToyRAG.Core.Splitting;
using ToyRAG.Core.Storage;

namespace ToyRAG.Core.Pipelines
{
    public class RAGPipeline(
        IDocumentLoader documentLoader,
        ITextSplitter textSplitter,
        IEmbeddingGenerator embeddingGenerator,
        IVectorStore vectorStore,
        IChatService chatService
    )
    {
        public async Task ImportAsync(string directoryPath, Action<int, int>? onProgress = null)
        {
            var docs = documentLoader.Load(directoryPath, onProgress);

            foreach (var doc in docs)
            {
                var chunks = textSplitter.Split(doc).ToList();
                if (chunks.Count == 0) continue;

                await embeddingGenerator.GenerateAsync(chunks);
                await vectorStore.SaveAsync(chunks);
            }
        }

        public async Task<string> QueryAsync(string question)
        {
            var queryEmbedding = embeddingGenerator.GenerateEmbeddings(question.ToLowerInvariant());

            var topResults = await vectorStore.SearchAsync(queryEmbedding);

            StringBuilder contextBuilder = new();
            contextBuilder.AppendLine("参考资料：");
            foreach (var (chunk, score) in topResults)
            {
                contextBuilder.AppendLine($"相关度: {score:F2}");
                contextBuilder.AppendLine($"数据来自：{chunk.Source}");
                contextBuilder.AppendLine(chunk.Content);
            }

            string message = $"问题：{question}\n{contextBuilder}";

            return await chatService.GenerateAsync(message);
        }
    }
}
