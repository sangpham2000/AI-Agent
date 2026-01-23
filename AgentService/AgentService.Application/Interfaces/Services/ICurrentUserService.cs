namespace AgentService.Application.Interfaces.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Username { get; }
    // Task<bool> HasPermissionAsync(string permissionCode); // For future use
}
