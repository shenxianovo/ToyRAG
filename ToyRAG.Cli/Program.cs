using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ToyRAG.Cli.Utils;
using ToyRAG.Core.Embeddings;
using ToyRAG.Core.Generation;
using ToyRAG.Core.Loading;
using ToyRAG.Core.Pipelines;
using ToyRAG.Core.Splitting;
using ToyRAG.Core.Storage;
using ToyRAG.Core.Tools;

// Configuration
string modelPath = @"E:\Code\Local\Learning\C#\ToyRAG\models\bge-m3\model.onnx";
string tokenizerPath = @"E:\Code\Local\Learning\C#\ToyRAG\models\bge-m3\tokenizer.json";
string testPath = "E:\\Code\\Local\\Learning\\C#\\ToyRAG\\data\\docs\\visual-basic\\getting-started";

// Interactive path selection or default
Console.WriteLine("请输入本地文件夹完整地址 (按回车使用默认测试路径)");
string? inputPath = Console.ReadLine();
string docsPath = string.IsNullOrWhiteSpace(inputPath) ? testPath : inputPath;

string gitHubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN", EnvironmentVariableTarget.User) 
    ?? throw new InvalidOperationException("GITHUB_TOKEN environment variable is not set.");

// Host Setup
var builder = Host.CreateApplicationBuilder(args);

// Register Core Services
builder.Services.AddSingleton<IDocumentLoader, MarkdownLoader>();
builder.Services.AddSingleton<ITextSplitter>(_ => new RecursiveCharacterTextSplitter { ChunkSize = 1000, ChunkOverlap = 100 });

// Register Embeddings (Scoped or Singleton depending on thread safety, assuming Singleton for now)
builder.Services.AddSingleton<IEmbeddingGenerator>(_ => new BgeM3EmbeddingGenerator(modelPath, tokenizerPath, true));

// Register Vector Store
builder.Services.AddSingleton<IVectorStore>(_ => new LocalVectorStrore("vectors.db"));

// Register Tools
builder.Services.AddTransient<RetrievalTool>();

// Register Chat Service with Middleware
builder.Services.AddSingleton<IChatService>(sp => 
    new GitHubChatService(
        gitHubToken,
        sp.GetRequiredService<RetrievalTool>()
    )
);

// Register Pipeline
builder.Services.AddSingleton<RAGPipeline>();

var host = builder.Build();

// Application Logic
var pipeline = host.Services.GetRequiredService<RAGPipeline>();

Console.WriteLine($"正在从 {docsPath} 导入文档...");
var watch = System.Diagnostics.Stopwatch.StartNew();
var progressBar = new ProgressBar(watch);

await pipeline.ImportAsync(docsPath, progressBar.ProgressCallback);

watch.Stop();
Console.WriteLine($"导入完成！耗时: {watch.Elapsed.TotalSeconds:F2} 秒");

// Chat Loop
while (true)
{
    Console.Write("\nUser > ");
    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input)) continue;
    if (input.Trim().ToLower() == "exit") break;

    Console.WriteLine("正在思考...");
    try
    {
        // The pipeline now delegates query refinement and search to the Agent
        string answer = await pipeline.QueryAsync(input);
        Console.WriteLine("\nAgent :");
        Console.WriteLine(answer);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n[错误]: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
    }
}
