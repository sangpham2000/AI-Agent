namespace AgentService.Application.DTOs;

public class AgentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FlowiseChatflowId { get; set; } = string.Empty;
    public string? SystemPrompt { get; set; }
    public string? FlowiseConfig { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
}

public class CreateAgentDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FlowiseChatflowId { get; set; } = string.Empty;
    public string? SystemPrompt { get; set; }
    public string? FlowiseConfig { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
}

public class UpdateAgentDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? FlowiseChatflowId { get; set; }
    public string? SystemPrompt { get; set; }
    public string? FlowiseConfig { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsDefault { get; set; }
}
