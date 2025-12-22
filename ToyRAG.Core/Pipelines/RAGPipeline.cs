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
                if (await vectorStore.IsDocumentIndexedAsync(doc.Source!))
                {
                    continue;
                }
                var chunks = textSplitter.Split(doc).ToList();
                if (chunks.Count == 0) continue;

                await embeddingGenerator.GenerateAsync(chunks);
                await vectorStore.SaveAsync(chunks);
            }
        }

        public async Task<string> QueryAsync(string question)
        {
            // Delegating the entire query process to the Agent (ChatService).
            // The Agent will use its tools to search the vector store if necessary.
            return await chatService.GenerateAsync(question);
        }
    }
}
