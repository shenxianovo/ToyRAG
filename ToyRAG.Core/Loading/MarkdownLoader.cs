using System.Text;
using ToyRAG.Core.Entities;

namespace ToyRAG.Core.Loading
{
    public class MarkdownLoader : IDocumentLoader
    {
        public IEnumerable<Document> Load(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException("Directory not found");

            foreach (var doc in TraverseDirectory(directoryPath))
                yield return doc;
        }

        private IEnumerable<Document> TraverseDirectory(string directoryPath)
        {
            foreach (var file in Directory.GetFiles(directoryPath))
            {
                yield return ParseDocument(file);
            }
            foreach (var directory in Directory.GetDirectories(directoryPath))
            {
                foreach (var doc in TraverseDirectory(directory))
                    yield return doc;
            }
        }

        private Document ParseDocument(string filePath)
        {
            var content = File.ReadAllLines(filePath);
            int id = 0;
            StringBuilder bodyBuilder = new();
            Dictionary<string, object> metaData = [];

            string? currentKey = null;
            List<string>? currentList = null;

            string state = "start";

            foreach (var line in content)
            {
            beginSwitch:
                switch (state)
                {
                    case "start":
                        if (line.Trim() == "---")
                            state = "yaml";
                        else
                        {
                            state = "body";
                            goto beginSwitch; // 重新处理这一行
                        }
                        break;

                    case "yaml":
                        if (line.Trim() == "---")
                        {
                            // YAML 结束
                            state = "body";
                            if (currentKey != null && currentList != null)
                                metaData[currentKey] = currentList;
                            break; // 结束当前行的处理，跳过这一行（即跳过结束的 ---）
                        }

                        if (string.IsNullOrWhiteSpace(line))
                            break;

                        var trimmed = line.Trim();

                        if (trimmed.StartsWith("- "))
                        {
                            if (currentList != null)
                                currentList.Add(trimmed.Substring(2).Trim('"'));
                        }
                        else if (trimmed.Contains(':'))
                        {
                            if (currentKey != null && currentList != null)
                            {
                                metaData[currentKey] = currentList;
                            }

                            int index = trimmed.IndexOf(':');
                            string key = trimmed.Substring(0, index).Trim();
                            string value = trimmed.Substring(index + 1).Trim();

                            if (string.IsNullOrEmpty(value))
                            {
                                currentKey = key;
                                currentList = new List<string>();
                            }
                            else
                            {
                                metaData[key] = value.Trim('"');
                                currentKey = null;
                                currentList = null;
                            }
                        }
                        break;

                    case "body":
                        bodyBuilder.AppendLine(line);
                        break;
                }
                id++;
            }
            return new Document
            {
                Id = id.ToString(),
                Body = bodyBuilder.ToString(),
                Source = filePath,
                MetaData = metaData
            };
        }
    }
}
