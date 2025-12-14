using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Vpassbackend.Models;

namespace Vpassbackend.Services
{
    /// <summary>
    /// Service for interacting with Groq LLM API (Llama 3.3 70B)
    /// </summary>
    public class GroqService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string GROQ_API_URL = "https://api.groq.com/openai/v1/chat/completions";
        private const string DEFAULT_MODEL = "llama-3.3-70b-versatile";

        public GroqService(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Groq:ApiKey"] ?? throw new ArgumentNullException("Groq API Key not configured");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<string> GenerateResponseAsync(
            string userQuestion,
            List<string> contextChunks,
            List<ChatMessage>? conversationHistory = null,
            string? systemPrompt = null)
        {
            var messages = new List<object>();

            // System prompt with context
            var context = string.Join("\n\n", contextChunks.Select((chunk, i) => $"[Context {i + 1}]\n{chunk}"));

            var defaultSystemPrompt = @"You are a helpful assistant for a vehicle service management system. 
Use the provided context to answer user questions accurately. 
If the context doesn't contain enough information, say so politely and provide general guidance.
Be concise, friendly, and professional.

Available Context:
" + context;

            messages.Add(new
            {
                role = "system",
                content = systemPrompt ?? defaultSystemPrompt
            });

            // Add conversation history if available
            if (conversationHistory != null && conversationHistory.Any())
            {
                foreach (var msg in conversationHistory.TakeLast(10)) // Last 10 messages for context
                {
                    messages.Add(new
                    {
                        role = msg.Role,
                        content = msg.Content
                    });
                }
            }

            // Add current user question
            messages.Add(new
            {
                role = "user",
                content = userQuestion
            });

            var requestBody = new
            {
                model = DEFAULT_MODEL,
                messages = messages,
                temperature = 0.7,
                max_tokens = 1024,
                top_p = 1,
                stream = false
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            // Retry logic for rate limits
            int maxRetries = 3;
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    var response = await _httpClient.PostAsync(GROQ_API_URL, content);

                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        if (i < maxRetries - 1)
                        {
                            await Task.Delay(2000 * (i + 1)); // Wait 2s, 4s, 6s
                            continue;
                        }
                    }

                    response.EnsureSuccessStatusCode();

                    var responseJson = await response.Content.ReadAsStringAsync();
                    var groqResponse = JsonSerializer.Deserialize<GroqChatResponse>(responseJson);

                    if (groqResponse?.Choices == null || groqResponse.Choices.Count == 0)
                    {
                        throw new Exception("Failed to generate response from Groq");
                    }

                    return groqResponse.Choices[0].Message.Content;
                }
                catch (HttpRequestException ex) when (ex.Message.Contains("429"))
                {
                    if (i < maxRetries - 1)
                    {
                        await Task.Delay(2000 * (i + 1));
                        continue;
                    }
                    throw new Exception("Groq rate limit exceeded. Please wait a moment and try again.");
                }
            }

            throw new Exception("Failed to generate response after retries");
        }

        private class GroqChatResponse
        {
            [JsonPropertyName("choices")]
            public List<GroqChoice> Choices { get; set; }
        }

        private class GroqChoice
        {
            [JsonPropertyName("message")]
            public GroqMessage Message { get; set; }
        }

        private class GroqMessage
        {
            [JsonPropertyName("content")]
            public string Content { get; set; }
        }
    }
}
