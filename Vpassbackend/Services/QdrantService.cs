using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vpassbackend.Services
{
    /// <summary>
    /// Service for interacting with Qdrant Vector Database
    /// </summary>
    public class QdrantService
    {
        private readonly HttpClient _httpClient;
        private readonly string _collectionName;
        private const int DEFAULT_TOP_K = 5;

        public QdrantService(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;
            var qdrantUrl = configuration["Qdrant:Url"] ?? "http://localhost:6333";
            var apiKey = configuration["Qdrant:ApiKey"];
            _collectionName = configuration["Qdrant:CollectionName"] ?? "vehicle_knowledge_base";

            _httpClient.BaseAddress = new Uri(qdrantUrl);
            
            if (!string.IsNullOrEmpty(apiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
            }
        }

        /// <summary>
        /// Create collection if it doesn't exist
        /// </summary>
        public async Task<bool> CreateCollectionAsync(int vectorSize = 1536)
        {
            var requestBody = new
            {
                vectors = new
                {
                    size = vectorSize,
                    distance = "Cosine"
                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PutAsync($"/collections/{_collectionName}", content);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Index a document with its embedding
        /// </summary>
        public async Task<string> IndexDocumentAsync(string id, float[] embedding, Dictionary<string, object> payload)
        {
            var requestBody = new
            {
                points = new[]
                {
                    new
                    {
                        id = id,
                        vector = embedding,
                        payload = payload
                    }
                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PutAsync($"/collections/{_collectionName}/points", content);
            response.EnsureSuccessStatusCode();

            return id;
        }

        /// <summary>
        /// Search for similar vectors using cosine similarity
        /// </summary>
        public async Task<List<QdrantSearchResult>> SearchAsync(
            float[] queryEmbedding, 
            int topK = DEFAULT_TOP_K,
            Dictionary<string, object>? filters = null,
            float scoreThreshold = 0.7f)
        {
            var requestBody = new
            {
                vector = queryEmbedding,
                limit = topK,
                with_payload = true,
                score_threshold = scoreThreshold,
                filter = filters
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"/collections/{_collectionName}/points/search", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonSerializer.Deserialize<QdrantSearchResponse>(responseJson);

            return searchResponse?.Result ?? new List<QdrantSearchResult>();
        }

        /// <summary>
        /// Delete a point by ID
        /// </summary>
        public async Task<bool> DeletePointAsync(string id)
        {
            var requestBody = new
            {
                points = new[] { id }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"/collections/{_collectionName}/points/delete", content);
            return response.IsSuccessStatusCode;
        }
    }

    public class QdrantSearchResponse
    {
        [JsonPropertyName("result")]
        public List<QdrantSearchResult> Result { get; set; }
    }

    public class QdrantSearchResult
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("score")]
        public float Score { get; set; }

        [JsonPropertyName("payload")]
        public Dictionary<string, JsonElement> Payload { get; set; }
    }
}
