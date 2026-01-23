namespace AgentService.Domain.Entities;

public class Conversation
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Platform { get; set; } = "web_plugin"; // "telegram", "web_plugin"
    public long? TelegramChatId { get; set; }
    public string? SessionId { get; set; } // For anonymous web users
    public string? Title { get; set; } // Auto-generated from first message
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? User { get; set; }
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
