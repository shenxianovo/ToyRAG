using System.Data.SQLite;
using System.Text.Json;
using ToyRAG.Core.Entities;

namespace ToyRAG.Core.Storage
{
    public class LocalVectorStrore : IVectorStore
    {
        private readonly string _connectionString;

        public LocalVectorStrore(string databasePath)
        {
            _connectionString = $"Data Source={databasePath};Version=3;";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Vectors (
                    Id TEXT PRIMARY KEY,
                    Content TEXT,
                    DocumentId TEXT,
                    Source TEXT,
                    MetaData TEXT,
                    Embedding BLOB
                );
            ";
            using var command = new SQLiteCommand(createTableQuery, connection);
            command.ExecuteNonQuery();
        }

        public async Task SaveAsync(IEnumerable<TextChunk> chunks)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();
            foreach (var chunk in chunks)
            {
                if (chunk.Embedding == null) continue;

                // 修改：插入所有字段，MetaData 序列化为 JSON
                string insertQuery = @"
                    INSERT OR REPLACE INTO Vectors (Id, Content, DocumentId, Source, MetaData, Embedding)
                    VALUES (@Id, @Content, @DocumentId, @Source, @MetaData, @Embedding);
                ";

                using var command = new SQLiteCommand(insertQuery, connection);
                // 处理可能为 null 的属性
                command.Parameters.AddWithValue("@Id", chunk.Id ?? Guid.NewGuid().ToString());
                command.Parameters.AddWithValue("@Content", (object)chunk.Content ?? DBNull.Value);
                command.Parameters.AddWithValue("@DocumentId", (object)chunk.DocumentId ?? DBNull.Value);
                command.Parameters.AddWithValue("@Source", (object)chunk.Source ?? DBNull.Value);

                // 序列化 MetaData
                string metaDataJson = chunk.MetaData != null ? JsonSerializer.Serialize(chunk.MetaData) : "{}";
                command.Parameters.AddWithValue("@MetaData", metaDataJson);

                command.Parameters.AddWithValue("@Embedding", ConvertToBlob(chunk.Embedding));

                await command.ExecuteNonQueryAsync();
            }
            transaction.Commit();
        }

        public async Task<IEnumerable<(TextChunk Chunk, float Score)>> SearchAsync(float[] queryEmbedding, int limit = 5)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            // 修改：查询所有字段
            string selectQuery = "SELECT Id, Content, DocumentId, Source, MetaData, Embedding FROM Vectors";
            using var command = new SQLiteCommand(selectQuery, connection);

            var results = new List<(TextChunk Chunk, float Score)>();

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                // 读取并还原数据
                var id = reader.IsDBNull(0) ? null : reader.GetString(0);
                var content = reader.IsDBNull(1) ? null : reader.GetString(1);
                var docId = reader.IsDBNull(2) ? null : reader.GetString(2);
                var source = reader.IsDBNull(3) ? null : reader.GetString(3);
                var metaJson = reader.IsDBNull(4) ? "{}" : reader.GetString(4);
                var embeddingBlob = (byte[])reader["Embedding"];

                var embedding = ConvertFromBlob(embeddingBlob);
                var metaData = JsonSerializer.Deserialize<Dictionary<string, object>>(metaJson) ?? new Dictionary<string, object>();

                float score = CalculateCosineSimilarity(queryEmbedding, embedding);

                results.Add((new TextChunk
                {
                    Id = id,
                    Content = content,
                    DocumentId = docId,
                    Source = source,
                    MetaData = metaData,
                    Embedding = embedding
                }, score));
            }

            return results
                .OrderByDescending(x => x.Score)
                .Take(limit);
        }

        public async Task<bool> IsDocumentIndexedAsync(string source)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            string checkQuery = "SELECT 1 FROM Vectors WHERE Source = @Source LIMIT 1";
            using var command = new SQLiteCommand(checkQuery, connection);
            command.Parameters.AddWithValue("@Source", source);

            var result = await command.ExecuteScalarAsync();
            return result != null;
        }

        private static byte[] ConvertToBlob(float[] embedding)
        {
            var byteArray = new byte[embedding.Length * sizeof(float)];
            Buffer.BlockCopy(embedding, 0, byteArray, 0, byteArray.Length);
            return byteArray;
        }

        private static float[] ConvertFromBlob(byte[] blob)
        {
            var floatArray = new float[blob.Length / sizeof(float)];
            Buffer.BlockCopy(blob, 0, floatArray, 0, blob.Length);
            return floatArray;
        }

        private static float CalculateCosineSimilarity(float[] vectorA, float[] vectorB)
        {
            if (vectorA.Length != vectorB.Length) return 0f;
            float dotProduct = 0f, magA = 0f, magB = 0f;
            for (int i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
                magA += vectorA[i] * vectorA[i];
                magB += vectorB[i] * vectorB[i];
            }
            if (magA == 0 || magB == 0) return 0f;
            return dotProduct / (float)(Math.Sqrt(magA) * Math.Sqrt(magB));
        }
    }
}
