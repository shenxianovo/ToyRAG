using System.Text.Json;

namespace ToyRAG.Core.Embeddings
{
    public class SimpleBgeTokenizer
    {
        private readonly Dictionary<string, int> _vocab;
        private readonly int _unkId;
        private readonly int _bosId;
        private readonly int _eosId;

        public SimpleBgeTokenizer(string tokenizerJsonPath)
        {
            var jsonContent = File.ReadAllText(tokenizerJsonPath);
            using var doc = JsonDocument.Parse(jsonContent);

            var modelEl = doc.RootElement.GetProperty("model");
            var vocabEl = modelEl.GetProperty("vocab");

            _vocab = new Dictionary<string, int>();

            // 格式: [ ["<s>", 0.0], ["<pad>", 0.0], ... ]
            // ID 就是数组的索引
            int index = 0;
            foreach (var item in vocabEl.EnumerateArray())
            {
                // item[0] 是 token 文本
                string token = item[0].GetString()!;

                // 存入字典: Token -> ID
                if (!_vocab.ContainsKey(token))
                {
                    _vocab[token] = index;
                }
                index++;
            }

            // 获取特殊 Token ID
            _bosId = _vocab.GetValueOrDefault("<s>", 0);
            _eosId = _vocab.GetValueOrDefault("</s>", 2);
            _unkId = _vocab.GetValueOrDefault("<unk>", 3);
        }

        public (long[] Ids, long[] Mask) Encode(string text)
        {
            if (string.IsNullOrEmpty(text))
                return (Array.Empty<long>(), Array.Empty<long>());

            var ids = new List<long>();

            // 添加开头 <s>
            ids.Add(_bosId);

            // 预处理：SentencePiece 用 U+2581 ( ) 代表空格
            // 简单处理：把所有空格换成  
            text = text.Replace(" ", "\u2581");

            // 如果开头不是空格，通常 SentencePiece 会在开头加一个  (取决于配置，BGE-M3 好像不需要强制加)
            // 为了保险，我们只处理文本内的空格

            // === 最大正向匹配算法 (Greedy Max Match) ===
            int i = 0;
            while (i < text.Length)
            {
                bool matched = false;
                // 尝试从当前位置匹配最长的词
                // 限制最大词长为 20 (优化性能，一般 token 不会太长)
                for (int len = Math.Min(20, text.Length - i); len > 0; len--)
                {
                    string sub = text.Substring(i, len);
                    if (_vocab.TryGetValue(sub, out int id))
                    {
                        ids.Add(id);
                        i += len;
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                {
                    // 没匹配到，当作 <unk>，前进 1 个字符
                    ids.Add(_unkId);
                    i++;
                }
            }

            // 添加结尾 </s>
            ids.Add(_eosId);

            // 构造 Mask (全 1)
            var mask = Enumerable.Repeat(1L, ids.Count).ToArray();

            return (ids.ToArray(), mask);
        }
    }
}