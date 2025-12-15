using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using ToyRAG.Core.Entities;

namespace ToyRAG.Core.Embeddings
{
    public class BgeM3EmbeddingGenerator : IEmbeddingGenerator
    {
        private readonly InferenceSession _session;
        private readonly SimpleBgeTokenizer _tokenizer;


        public BgeM3EmbeddingGenerator(string modelPath, string tokenizerJsonPath, bool useGpu = false)
        {
            _tokenizer = new SimpleBgeTokenizer(tokenizerJsonPath);

            var sessionOptions = new SessionOptions();
            if (useGpu)
            {
                sessionOptions.AppendExecutionProvider_CUDA(); // CUDA12.x，CUDA13会报错...
            }

            _session = new InferenceSession(modelPath, sessionOptions);
        }

        public Task GenerateAsync(IEnumerable<TextChunk> chunks)
        {
            foreach (var chunk in chunks)
            {
                if (string.IsNullOrWhiteSpace(chunk.Content)) continue;
                chunk.Embedding = GenerateEmbeddings(chunk.Content);
            }
            return Task.CompletedTask;
        }

        public float[] GenerateEmbeddings(string text)
        {
            // 1. 使用手写 Tokenizer 分词
            var (inputIds, attentionMask) = _tokenizer.Encode(text);

            // 2. 构造输入
            var dimensions = new[] { 1, inputIds.Length };
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", new DenseTensor<long>(inputIds, dimensions)),
                NamedOnnxValue.CreateFromTensor("attention_mask", new DenseTensor<long>(attentionMask, dimensions))
            };

            // 3. 推理
            using var results = _session.Run(inputs);
            var outputTensor = results.First().AsTensor<float>();

            // 4. 获取 [CLS] 向量 (Index 0)
            int hiddenSize = outputTensor.Dimensions[2];
            float[] embedding = new float[hiddenSize];
            for (int i = 0; i < hiddenSize; i++)
            {
                embedding[i] = outputTensor[0, 0, i];
            }

            // 5. 归一化
            return Normalize(embedding);
        }

        private float[] Normalize(float[] vector)
        {
            float sumSquares = 0;
            foreach (var val in vector) sumSquares += val * val;
            float norm = (float)Math.Sqrt(sumSquares);
            if (norm > 1e-9)
                for (int i = 0; i < vector.Length; i++) vector[i] /= norm;
            return vector;
        }
    }
}
