namespace AgentService.Domain.Entities.Auth;

public class Permission
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty; // e.g., "USER_VIEW", "DATA_VIEW_ALL"
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty; // e.g., "System", "UserManagement"
}

