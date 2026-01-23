namespace AgentService.Domain.Entities.Auth;

public class RolePermission
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public string PermissionCode { get; set; } = string.Empty;
    public Permission Permission { get; set; } = null!;
}
