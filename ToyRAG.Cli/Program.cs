using ToyRAG.Cli.Utils;
using ToyRAG.Core.Embeddings;
using ToyRAG.Core.Generation;
using ToyRAG.Core.Loading;
using ToyRAG.Core.Pipelines;
using ToyRAG.Core.Splitting;
using ToyRAG.Core.Storage;

//string modelPath = @"E:\Code\Local\Learning\C#\ToyRAG\models\all-MiniLM-L12-v2\all-MiniLM-L12-v2.onnx";
string modelPath = @"E:\Code\Local\Learning\C#\ToyRAG\models\bge-m3\model.onnx";
//string vocabPath = @"E:\Code\Local\Learning\C#\ToyRAG\models\all-MiniLM-L12-v2\vocab.txt";
string tokenizerPath = @"E:\Code\Local\Learning\C#\ToyRAG\models\bge-m3\tokenizer.json";
Console.WriteLine("请输入本地文件夹完整地址");
//string docsPath = Console.ReadLine()!;
string testPath = "E:\\Code\\Local\\Learning\\C#\\ToyRAG\\data\\docs\\visual-basic\\getting-started";
string docsPath = testPath;

string gitHubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN", EnvironmentVariableTarget.User)!;

IDocumentLoader documentLoader = new MarkdownLoader();
ITextSplitter textSplitter = new FixedTextSplitter() { ChunkSize = 1000 };
//IEmbeddingGenerator embeddingGenerator = new MiniLMEmbeddingGenerator(modelPath, vocabPath, true);
IEmbeddingGenerator embeddingGenerator = new BgeM3EmbeddingGenerator(modelPath, tokenizerPath, true);
//IVectorStore vectorStore = new InMemoryVectorStore();
IVectorStore vectorStore = new LocalVectorStrore("vectors.db");
IChatService chatService = new GitHubChatService(gitHubToken, middleware:Middleware.FunctionCallMiddleware);

RAGPipeline pipeline = new(
    documentLoader,
    textSplitter,
    embeddingGenerator,
    vectorStore,
    chatService
);

Console.WriteLine($"正在从 {docsPath} 导入文档...");
var watch = System.Diagnostics.Stopwatch.StartNew();
var progressBar = new ProgressBar(watch);
await pipeline.ImportAsync(docsPath, progressBar.ProgressCallback);
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