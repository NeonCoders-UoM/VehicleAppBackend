using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Vpassbackend.Services
{
    /// <summary>
    /// Service for generating embeddings using OpenAI text-embedding-3-small
    /// </summary>
    public class OpenAIEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string EMBEDDING_MODEL = "text-embedding-3-small";
        private const string OPENAI_API_URL = "https://api.openai.com/v1/embeddings";

        public OpenAIEmbeddingService(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI API Key not configured");
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            var requestBody = new
            {
                input = text,
                model = EMBEDDING_MODEL
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            // Create request with auth header
            var request = new HttpRequestMessage(HttpMethod.Post, OPENAI_API_URL);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var embeddingResponse = JsonSerializer.Deserialize<OpenAIEmbeddingResponse>(responseJson);

            if (embeddingResponse?.Data == null || embeddingResponse.Data.Count == 0)
            {
                throw new Exception("Failed to generate embedding");
            }

            return embeddingResponse.Data[0].Embedding;
        }

        public async Task<List<float[]>> GenerateBatchEmbeddingsAsync(List<string> texts)
        {
            var requestBody = new
            {
                input = texts,
                model = EMBEDDING_MODEL
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            // Create request with auth header
            var request = new HttpRequestMessage(HttpMethod.Post, OPENAI_API_URL);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var embeddingResponse = JsonSerializer.Deserialize<OpenAIEmbeddingResponse>(responseJson);

            if (embeddingResponse?.Data == null)
            {
                throw new Exception("Failed to generate embeddings");
            }

            return embeddingResponse.Data.Select(d => d.Embedding).ToList();
        }

        private class OpenAIEmbeddingResponse
        {
            public List<EmbeddingData> Data { get; set; }
        }

        private class EmbeddingData
        {
            public float[] Embedding { get; set; }
        }
    }
}
