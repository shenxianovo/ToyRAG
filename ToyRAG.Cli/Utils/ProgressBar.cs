using System.Collections.Concurrent;

namespace ToyRAG.Cli.Utils
{
    public class ProgressBar
    {
        private ConcurrentQueue<double> _timeIntervals;
        private double _smoothedAvgTimePerDoc;
        private System.Diagnostics.Stopwatch _watch;

        public ProgressBar(System.Diagnostics.Stopwatch watch)
        {
            _watch = watch;
            _timeIntervals = new();
            _smoothedAvgTimePerDoc = 0;
        }

        public void ProgressCallback(int current, int total)
        {
            int barSize = 30;
            double percent = (double)current / total;
            int filled = (int)(percent * barSize);

            string bar = new string('=', filled) + new string(' ', barSize - filled);

            if (current > 1)
            {
                double elapsedTime = _watch.Elapsed.TotalSeconds;
                double timePerDoc = elapsedTime / current;
                _timeIntervals.Enqueue(timePerDoc);

                if (_timeIntervals.Count > 10) // 保留最近 10 次的时间间隔
                {
                    _timeIntervals.TryDequeue(out _);
                }

                // 使用加权移动平均平滑时间
                _smoothedAvgTimePerDoc = _smoothedAvgTimePerDoc == 0
                    ? timePerDoc
                    : _smoothedAvgTimePerDoc * 0.9 + timePerDoc * 0.1;
            }

            // 计算剩余时间
            int remainingDocs = total - current;
            TimeSpan remainingTime = TimeSpan.FromSeconds(_smoothedAvgTimePerDoc * remainingDocs);

            Console.Write($"\r[{bar}] {current}/{total} ({percent:P0}) 预计剩余时间: {remainingTime:mm\\:ss}");
        }
    }
}
