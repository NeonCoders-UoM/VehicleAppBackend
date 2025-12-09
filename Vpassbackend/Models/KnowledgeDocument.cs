using System.ComponentModel.DataAnnotations;

namespace Vpassbackend.Models
{
    /// <summary>
    /// Represents documents stored in the knowledge base for RAG
    /// </summary>
    public class KnowledgeDocument
    {
        [Key]
        public int DocumentId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public string Category { get; set; } // e.g., "services", "appointments", "general"

        public string QdrantId { get; set; } // Reference to Qdrant vector ID

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}
