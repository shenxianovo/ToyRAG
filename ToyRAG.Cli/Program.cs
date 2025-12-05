using Google.Protobuf.WellKnownTypes;
using System.Collections.Concurrent;
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
//string docsPath = @"E:\Code\Local\Learning\C#\ToyRAG\data\docs";
string docsPath = @"E:\Code\Local\Projects\WHUDAYS.github.io";

string gitHubToken = "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";

IDocumentLoader documentLoader = new MarkdownLoader();
ITextSplitter textSplitter = new FixedTextSplitter() { ChunkSize = 1000 };
//IEmbeddingGenerator embeddingGenerator = new OnnxEmbeddingGenerator(modelPath, vocabPath, true);
IEmbeddingGenerator embeddingGenerator = new BgeM3EmbeddingGenerator(modelPath, tokenizerPath, true);
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

// 进度条
ConcurrentQueue<double> timeIntervals = new();
double smoothedAvgTimePerDoc = 0;
Action<int, int> progressCallback = (current, total) =>
{
    int barSize = 30;
    double percent = (double)current / total;
    int filled = (int)(percent * barSize);

    string bar = new string('=', filled) + new string(' ', barSize - filled);

    if (current > 1)
    {
        double elapsedTime = watch.Elapsed.TotalSeconds;
        double timePerDoc = elapsedTime / current;
        timeIntervals.Enqueue(timePerDoc);

        if (timeIntervals.Count > 10) // 保留最近 10 次的时间间隔
        {
            timeIntervals.TryDequeue(out _);
        }

        // 使用加权移动平均平滑时间
        smoothedAvgTimePerDoc = smoothedAvgTimePerDoc == 0
            ? timePerDoc
            : smoothedAvgTimePerDoc * 0.9 + timePerDoc * 0.1;
    }

    // 计算剩余时间
    int remainingDocs = total - current;
    TimeSpan remainingTime = TimeSpan.FromSeconds(smoothedAvgTimePerDoc * remainingDocs);

    Console.Write($"\r[{bar}] {current}/{total} ({percent:P0}) 预计剩余时间: {remainingTime:mm\\:ss}");
};

await pipeline.ImportAsync(docsPath, progressCallback);
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