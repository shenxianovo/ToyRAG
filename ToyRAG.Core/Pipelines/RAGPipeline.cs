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
                contextBuilder.AppendLine($"--- (相关度: {score:F2}) ---");
                contextBuilder.AppendLine(chunk.Content);
            }

            string systemPrompt = "你是一个智能助手。请根据提供的参考资料回答用户的问题。如果参考资料中没有答案，请直接说不知道。";
            string userPrompt = $"问题：{question}\n{contextBuilder.ToString()}";

            return await chatService.GenerateAsync(systemPrompt, userPrompt);
        }
    }
}
