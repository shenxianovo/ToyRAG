using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.Tokenizers;
using Microsoft.ML.OnnxRuntime.Tensors;
using ToyRAG.Core.Entities;

namespace ToyRAG.Core.Embeddings
{
    public class OnnxEmbeddingGenerator : IEmbeddingGenerator
    {
        private readonly InferenceSession _session;
        private readonly BertTokenizer _tokenizer;

        public OnnxEmbeddingGenerator(string modelPath, string vocabPath)
        {
            _session = new(modelPath);
            _tokenizer = BertTokenizer.Create(vocabPath);
        }
        public async Task GenerateAsync(IEnumerable<TextChunk> chunks)
        {
            foreach (var chunk in chunks)
            {
                if (string.IsNullOrEmpty(chunk.Content)) continue;

                var text = chunk.Content.ToLowerInvariant();

                chunk.Embedding = GenerateEmbeddings(text);
            }
        }

        public float[] GenerateEmbeddings(string text)
        {
            var tokens = _tokenizer.EncodeToIds(text);

            int sequenceLength = tokens.Count + 2;
            long[] inputIds = new long[sequenceLength];
            long[] attentionMask = new long[sequenceLength];
            long[] tokenTypeIds = new long[sequenceLength];

            inputIds[0] = 101; // [CLS]
            attentionMask[0] = 1;

            for (int i = 0; i < tokens.Count; i++)
            {
                inputIds[i + 1] = tokens[i];
                attentionMask[i + 1] = 1;
            }

            inputIds[sequenceLength - 1] = 102; // [SEP]
            attentionMask[sequenceLength - 1] = 1;

            // 创建 Tensors
            var dimensions = new[] { 1, sequenceLength };
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", new DenseTensor<long>(inputIds, dimensions)),
                NamedOnnxValue.CreateFromTensor("attention_mask", new DenseTensor<long>(attentionMask, dimensions)),
                NamedOnnxValue.CreateFromTensor("token_type_ids", new DenseTensor<long>(tokenTypeIds, dimensions))
            };

            using var results = _session.Run(inputs);
            var outputTensor = results.First().AsTensor<float>();

            return MeanPoolingAndNormalize(outputTensor, attentionMask);
        }

        private float[] MeanPoolingAndNormalize(Tensor<float> output, long[] attentionMask)
        {
            int seqLen = output.Dimensions[1];
            int hiddenSize = output.Dimensions[2];
            float[] pooled = new float[hiddenSize];
            int validTokenCount = 0;

            // Mean Pooling
            for (int i = 0; i < seqLen; i++)
            {
                if (attentionMask[i] == 1)
                {
                    for (int j = 0; j < hiddenSize; j++)
                    {
                        pooled[j] += output[0, i, j];
                    }
                    validTokenCount++;
                }
            }

            for (int i = 0; i < hiddenSize; i++)
            {
                pooled[i] /= Math.Max(validTokenCount, 1);
            }

            // Normalize
            float sumSquares = 0;
            foreach (var val in pooled) sumSquares += val * val;
            float norm = (float)Math.Sqrt(sumSquares);

            if (norm > 1e-9)
            {
                for (int i = 0; i < hiddenSize; i++) pooled[i] /= norm;
            }

            return pooled;
        }
    }
}
