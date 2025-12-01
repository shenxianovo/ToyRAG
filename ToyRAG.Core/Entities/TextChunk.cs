namespace ToyRAG.Core.Entities
{
    public class TextChunk
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string DocumentId { get; set; }
        public string Source { get; set; }
        public float[] Embedding { get; set; }
        public Dictionary<string, object> MetaData { get; set; }

    }
}
