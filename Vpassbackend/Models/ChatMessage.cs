using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vpassbackend.Models
{
    public class ChatMessage
    {
        [Key]
        public int MessageId { get; set; }

        public int ConversationId { get; set; }

        [Required]
        public string Role { get; set; } // "user" or "assistant"

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("ConversationId")]
        public ChatConversation Conversation { get; set; }
    }
}
