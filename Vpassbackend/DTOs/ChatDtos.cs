namespace Vpassbackend.DTOs
{
    public class ChatRequestDto
    {
        public string Message { get; set; }
        public string? SessionId { get; set; } // Optional for continuing conversations
        public int? CustomerId { get; set; } // Optional for authenticated users
        public int? UserId { get; set; } // Optional for staff users
    }

    public class ChatResponseDto
    {
        public string Response { get; set; }
        public string SessionId { get; set; }
        public int ConversationId { get; set; }
        public List<string> RetrievedContext { get; set; } = new List<string>();
        public int ContextChunksUsed { get; set; }
    }

    public class ChatHistoryDto
    {
        public int MessageId { get; set; }
        public string Role { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ConversationDto
    {
        public int ConversationId { get; set; }
        public string SessionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastMessageAt { get; set; }
        public List<ChatHistoryDto> Messages { get; set; } = new List<ChatHistoryDto>();
    }
}
