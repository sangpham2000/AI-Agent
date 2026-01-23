namespace AgentService.Domain.Entities;

public class Message
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public string Role { get; set; } = "user"; // "user", "assistant", "system"
    public string Content { get; set; } = string.Empty;
    public string? Metadata { get; set; } // JSON: citations, sources, tokens used
    public int? TokensUsed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Conversation Conversation { get; set; } = null!;
}
