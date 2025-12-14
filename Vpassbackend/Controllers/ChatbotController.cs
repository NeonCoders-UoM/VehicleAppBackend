using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vpassbackend.DTOs;
using Vpassbackend.Services;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        private readonly ChatbotService _chatbotService;
        private readonly PdfKnowledgeService _pdfKnowledgeService;
        private readonly ILogger<ChatbotController> _logger;

        public ChatbotController(
            ChatbotService chatbotService, 
            PdfKnowledgeService pdfKnowledgeService,
            ILogger<ChatbotController> logger)
        {
            _chatbotService = chatbotService;
            _pdfKnowledgeService = pdfKnowledgeService;
            _logger = logger;
        }

        /// <summary>
        /// Main chat endpoint - processes user questions through RAG pipeline
        /// </summary>
        [HttpPost("chat")]
        [AllowAnonymous]
        public async Task<IActionResult> Chat([FromBody] ChatRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Message cannot be empty");
            }

            try
            {
                var (response, contextChunks, conversationId) = await _chatbotService.ProcessQuestionAsync(
                    question: request.Message,
                    sessionId: request.SessionId,
                    customerId: request.CustomerId,
                    userId: request.UserId,
                    topK: 5
                );

                var chatResponse = new ChatResponseDto
                {
                    Response = response,
                    SessionId = request.SessionId ?? Guid.NewGuid().ToString(),
                    ConversationId = conversationId,
                    RetrievedContext = contextChunks,
                    ContextChunksUsed = contextChunks.Count
                };

                return Ok(chatResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat request");
                return StatusCode(500, new { error = "Failed to process chat request", details = ex.Message });
            }
        }

        /// <summary>
        /// Get conversation history by session ID
        /// </summary>
        [HttpGet("conversation/{sessionId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetConversation(string sessionId)
        {
            try
            {
                var conversation = await _chatbotService.GetConversationAsync(sessionId);

                if (conversation == null)
                {
                    return NotFound("Conversation not found");
                }

                var conversationDto = new ConversationDto
                {
                    ConversationId = conversation.ConversationId,
                    SessionId = conversation.SessionId,
                    CreatedAt = conversation.CreatedAt,
                    LastMessageAt = conversation.LastMessageAt,
                    Messages = conversation.Messages.Select(m => new ChatHistoryDto
                    {
                        MessageId = m.MessageId,
                        Role = m.Role,
                        Content = m.Content,
                        CreatedAt = m.CreatedAt
                    }).ToList()
                };

                return Ok(conversationDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversation");
                return StatusCode(500, new { error = "Failed to retrieve conversation" });
            }
        }

        /// <summary>
        /// Index knowledge base documents into Qdrant (Admin only)
        /// </summary>
        [HttpPost("index-knowledge")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> IndexKnowledge()
        {
            try
            {
                var indexedCount = await _chatbotService.IndexKnowledgeDocumentsAsync();
                return Ok(new { message = $"Successfully indexed {indexedCount} documents", count = indexedCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing knowledge documents");
                return StatusCode(500, new { error = "Failed to index knowledge documents", details = ex.Message });
            }
        }

        /// <summary>
        /// Health check endpoint for chatbot services
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                services = new
                {
                    openai = "configured",
                    qdrant = "configured",
                    groq = "configured"
                }
            });
        }

        /// <summary>
        /// Upload PDF and add to knowledge base (Admin only)
        /// </summary>
        [HttpPost("upload-pdf")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> UploadPdf([FromForm] PdfUploadDto request)
        {
            if (request.PdfFile == null || request.PdfFile.Length == 0)
            {
                return BadRequest("PDF file is required");
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest("Title is required");
            }

            // Validate file type
            if (!request.PdfFile.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only PDF files are allowed");
            }

            try
            {
                int documentsAdded;
                
                using (var stream = request.PdfFile.OpenReadStream())
                {
                    documentsAdded = await _pdfKnowledgeService.ProcessPdfStreamAsync(
                        pdfStream: stream,
                        title: request.Title,
                        category: request.Category,
                        autoChunk: request.AutoChunk,
                        maxChunkSize: request.MaxChunkSize
                    );
                }

                // Auto-index after upload
                var indexed = await _chatbotService.IndexKnowledgeDocumentsAsync();

                return Ok(new PdfUploadResponseDto
                {
                    Success = true,
                    DocumentsAdded = documentsAdded,
                    DocumentsIndexed = indexed,
                    Message = $"Successfully processed PDF and added {documentsAdded} document(s) to knowledge base. Indexed {indexed} total documents."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading PDF");
                return StatusCode(500, new PdfUploadResponseDto
                {
                    Success = false,
                    Message = "Failed to process PDF",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}
