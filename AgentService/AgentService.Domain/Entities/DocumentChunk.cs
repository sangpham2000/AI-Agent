namespace AgentService.Domain.Entities;

public class DocumentChunk
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public int ChunkIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? EmbeddingId { get; set; } // Reference to vector store
    public int? PageNumber { get; set; }
    public string? Metadata { get; set; } // JSON: additional chunk info
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Document Document { get; set; } = null!;
}
