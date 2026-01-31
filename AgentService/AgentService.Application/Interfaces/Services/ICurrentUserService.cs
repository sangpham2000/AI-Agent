namespace AgentService.Application.Interfaces.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Username { get; }
    string? Email { get; }
    string? FirstName { get; }
    string? LastName { get; }
    // Task<bool> HasPermissionAsync(string permissionCode); // For future use
}
