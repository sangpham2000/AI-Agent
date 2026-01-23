namespace AgentService.Domain.Entities;

public class Document
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty; // "pdf", "docx"
    public string? FilePath { get; set; }
    public string? Category { get; set; } // "quy_che", "giao_trinh", "hoc_lieu"
    public string? Description { get; set; }
    public long FileSize { get; set; }
    public Guid? UploadedByUserId { get; set; }
    public bool IsProcessed { get; set; } = false;
    public string? ProcessingError { get; set; }
    public int ChunkCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? UploadedBy { get; set; }
    public ICollection<DocumentChunk> Chunks { get; set; } = new List<DocumentChunk>();
}
