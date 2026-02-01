namespace AgentService.Domain.Entities;

public class Agent
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Flowise Configuration
    public string FlowiseChatflowId { get; set; } = string.Empty;
    
    // Configurable Parameters
    public string? SystemPrompt { get; set; }
    public string? FlowiseConfig { get; set; } // JSON string for overrides
    
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
