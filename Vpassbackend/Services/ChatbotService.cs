using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.Models;

namespace Vpassbackend.Services
{
    /// <summary>
    /// Orchestrates the RAG pipeline: Embedding -> Qdrant Search -> Groq LLM
    /// </summary>
    public class ChatbotService
    {
        private readonly ApplicationDbContext _context;
        private readonly OpenAIEmbeddingService _embeddingService;
        private readonly QdrantService _qdrantService;
        private readonly GroqService _groqService;
        private readonly ILogger<ChatbotService> _logger;

        public ChatbotService(
            ApplicationDbContext context,
            OpenAIEmbeddingService embeddingService,
            QdrantService qdrantService,
            GroqService groqService,
            ILogger<ChatbotService> logger)
        {
            _context = context;
            _embeddingService = embeddingService;
            _qdrantService = qdrantService;
            _groqService = groqService;
            _logger = logger;
        }

        /// <summary>
        /// Process user question through the RAG pipeline
        /// </summary>
        public async Task<(string response, List<string> contextChunks, int conversationId)> ProcessQuestionAsync(
            string question,
            string? sessionId = null,
            int? customerId = null,
            int? userId = null,
            int topK = 5)
        {
            try
            {
                // 1. Get or create conversation
                var conversation = await GetOrCreateConversationAsync(sessionId, customerId, userId);

                // 2. Generate embedding for user question
                _logger.LogInformation("Generating embedding for question: {Question}", question);
                var questionEmbedding = await _embeddingService.GenerateEmbeddingAsync(question);

                // 3. Search Qdrant for relevant context
                _logger.LogInformation("Searching Qdrant for relevant context (top {TopK})", topK);
                var searchResults = await _qdrantService.SearchAsync(
                    queryEmbedding: questionEmbedding,
                    topK: topK,
                    scoreThreshold: 0.6f
                );

                // 4. Extract context chunks from search results
                var contextChunks = searchResults
                    .Select(r => 
                    {
                        var content = r.Payload.ContainsKey("content") 
                            ? r.Payload["content"].GetString() 
                            : "";
                        var title = r.Payload.ContainsKey("title") 
                            ? r.Payload["title"].GetString() 
                            : "";
                        return $"{title}\n{content}";
                    })
                    .ToList();

                _logger.LogInformation("Retrieved {Count} context chunks", contextChunks.Count);

                // 5. Get conversation history
                var conversationHistory = await _context.ChatMessages
                    .Where(m => m.ConversationId == conversation.ConversationId)
                    .OrderBy(m => m.CreatedAt)
                    .ToListAsync();

                // 6. Generate response using Groq with context
                _logger.LogInformation("Generating response using Groq LLM");
                var response = await _groqService.GenerateResponseAsync(
                    userQuestion: question,
                    contextChunks: contextChunks,
                    conversationHistory: conversationHistory
                );

                // 7. Save user message
                var userMessage = new ChatMessage
                {
                    ConversationId = conversation.ConversationId,
                    Role = "user",
                    Content = question,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ChatMessages.Add(userMessage);

                // 8. Save assistant response
                var assistantMessage = new ChatMessage
                {
                    ConversationId = conversation.ConversationId,
                    Role = "assistant",
                    Content = response,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ChatMessages.Add(assistantMessage);

                // 9. Update conversation timestamp
                conversation.LastMessageAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return (response, contextChunks, conversation.ConversationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chatbot question");
                throw;
            }
        }

        /// <summary>
        /// Index knowledge documents into Qdrant
        /// </summary>
        public async Task<int> IndexKnowledgeDocumentsAsync()
        {
            var documents = await _context.KnowledgeDocuments
                .Where(d => d.IsActive)
                .ToListAsync();

            int indexed = 0;
            foreach (var doc in documents)
            {
                try
                {
                    // Generate embedding
                    var embedding = await _embeddingService.GenerateEmbeddingAsync(doc.Content);

                    // Prepare payload
                    var payload = new Dictionary<string, object>
                    {
                        { "title", doc.Title },
                        { "content", doc.Content },
                        { "category", doc.Category ?? "general" },
                        { "documentId", doc.DocumentId }
                    };

                    // Index in Qdrant
                    var qdrantId = await _qdrantService.IndexDocumentAsync(
                        id: doc.DocumentId.ToString(),
                        embedding: embedding,
                        payload: payload
                    );

                    doc.QdrantId = qdrantId;
                    doc.UpdatedAt = DateTime.UtcNow;
                    indexed++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to index document {DocumentId}", doc.DocumentId);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Indexed {Count} documents", indexed);

            return indexed;
        }

        /// <summary>
        /// Get or create a conversation session
        /// </summary>
        private async Task<ChatConversation> GetOrCreateConversationAsync(
            string? sessionId,
            int? customerId,
            int? userId)
        {
            ChatConversation? conversation = null;

            // Try to find existing conversation by sessionId
            if (!string.IsNullOrEmpty(sessionId))
            {
                conversation = await _context.ChatConversations
                    .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.IsActive);
            }

            // Create new conversation if not found
            if (conversation == null)
            {
                conversation = new ChatConversation
                {
                    SessionId = sessionId ?? Guid.NewGuid().ToString(),
                    CustomerId = customerId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    LastMessageAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.ChatConversations.Add(conversation);
                await _context.SaveChangesAsync();
            }

            return conversation;
        }

        /// <summary>
        /// Get conversation history
        /// </summary>
        public async Task<ChatConversation?> GetConversationAsync(string sessionId)
        {
            return await _context.ChatConversations
                .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);
        }
    }
}
