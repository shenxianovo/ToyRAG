using ToyRAG.Core.Embeddings;
using ToyRAG.Core.Generation;
using ToyRAG.Core.Loading;
using ToyRAG.Core.Pipelines;
using ToyRAG.Core.Splitting;
using ToyRAG.Core.Storage;

string modelPath = @"E:\Code\Local\Learning\C#\ToyRAG\models\all-MiniLM-L12-v2.onnx";
string vocabPath = @"E:\Code\Local\Learning\C#\ToyRAG\models\vocab.txt";
string docsPath = @"E:\Code\Local\Learning\C#\ToyRAG\data\docs\standard\serialization";

string gitHubToken = "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";

IDocumentLoader documentLoader = new MarkdownLoader();
ITextSplitter textSplitter = new FixedTextSplitter();
IEmbeddingGenerator embeddingGenerator = new OnnxEmbeddingGenerator(modelPath, vocabPath);
IVectorStore vectorStore = new InMemoryVectorStore();
IChatService chatService = new GitHubChatService(gitHubToken);

RAGPipeline pipeline = new(
    documentLoader,
    textSplitter,
    embeddingGenerator,
    vectorStore,
    chatService
);

Console.WriteLine($"正在从 {docsPath} 导入文档...");
var watch = System.Diagnostics.Stopwatch.StartNew();
await pipeline.ImportAsync(docsPath);
watch.Stop();
Console.WriteLine($"导入完成！耗时: {watch.Elapsed.TotalSeconds:F2} 秒");

while (true)
{
    Console.Write("\nUser > ");
    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input)) continue;
    if (input.Trim().ToLower() == "exit") break;

    Console.WriteLine("正在思考...");
    try
    {
        string answer = await pipeline.QueryAsync(input);
        Console.WriteLine("\nAgent :");
        Console.WriteLine(answer);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n[错误]: {ex.Message}");
    }
}