namespace ToyRAG.Core.Entities
{
    public class Document
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string Source { get; set; }
        public Dictionary<string, object> MetaData { get; set; }
    }
}
