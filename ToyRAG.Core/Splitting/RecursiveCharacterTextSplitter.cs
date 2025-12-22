using System.Text.RegularExpressions;
using ToyRAG.Core.Entities;

namespace ToyRAG.Core.Splitting
{
    public class RecursiveCharacterTextSplitter : ITextSplitter
    {
        public int ChunkSize { get; set; } = 1000;
        public int ChunkOverlap { get; set; } = 100;
        // Default separators: Paragraphs, Newlines, Spaces, Characters
        public string[] Separators { get; set; } = ["\n\n", "\n", " ", ""];

        public IEnumerable<TextChunk> Split(Document document)
        {
            if (string.IsNullOrEmpty(document.Body))
            {
                yield break;
            }

            var chunks = SplitText(document.Body, Separators);
            int chunkId = 0;

            foreach (var chunkText in chunks)
            {
                yield return new TextChunk
                {
                    Id = $"{document.Id}_{chunkId}",
                    Content = chunkText,
                    DocumentId = document.Id,
                    Source = document.Source,
                    MetaData = document.MetaData
                };
                chunkId++;
            }
        }

        private List<string> SplitText(string text, string[] separators)
        {
            List<string> finalChunks = new();
            
            // 1. Find the appropriate separator
            string separator = separators.Last(); // Default to empty string (char split)
            string[] newSeparators = [];
            
            foreach (var sep in separators)
            {
                if (string.IsNullOrEmpty(sep)) continue;
                if (text.Contains(sep))
                {
                    separator = sep;
                    // Next level of separators are those following the current one
                    newSeparators = separators.Skip(Array.IndexOf(separators, sep) + 1).ToArray();
                    break;
                }
            }

            // 2. Split text
            // If separator is empty, we are at character level
            string[] splits;
            if (string.IsNullOrEmpty(separator))
            {
                splits = text.Select(c => c.ToString()).ToArray();
            }
            else
            {
                splits = text.Split(new[] { separator }, StringSplitOptions.None);
            }

            // 3. Merge splits into chunks
            List<string> goodSplits = new();
            foreach (var s in splits)
            {
                if (string.IsNullOrEmpty(s)) continue;
                
                // If a single split is larger than ChunkSize, we need to recurse on it
                if (s.Length > ChunkSize && newSeparators.Length > 0)
                {
                    var subChunks = SplitText(s, newSeparators);
                    goodSplits.AddRange(subChunks);
                }
                else
                {
                    goodSplits.Add(s);
                }
            }

            // 4. Combine good splits into final chunks respecting ChunkSize and Overlap
            string currentChunk = "";
            foreach (var split in goodSplits)
            {
                // Restore separator for non-empty separators if it's not the last one
                // Note: simplified logic here, strict reconstruction might need keeping track of exact positions
                // For RAG, adding a space or newline is usually fine.
                string segment = string.IsNullOrEmpty(currentChunk) ? split : (string.IsNullOrEmpty(separator) ? split : separator + split);
                
                // Check if adding this segment would exceed chunk size
                // Note: We use length of currentChunk + length of separator + length of split
                int predictedLength = currentChunk.Length + (string.IsNullOrEmpty(currentChunk) ? 0 : separator.Length) + split.Length;

                if (predictedLength > ChunkSize)
                {
                    if (!string.IsNullOrEmpty(currentChunk))
                    {
                        finalChunks.Add(currentChunk);
                        
                        // Handle Overlap (Simple version: just keep the last segment if it fits)
                        // A more complex version would look back X chars.
                        // Here we just start fresh with the current split.
                    }
                    currentChunk = split;
                }
                else
                {
                    currentChunk = string.IsNullOrEmpty(currentChunk) ? split : currentChunk + separator + split;
                }
            }

            if (!string.IsNullOrEmpty(currentChunk))
            {
                finalChunks.Add(currentChunk);
            }

            return finalChunks;
        }
    }
}