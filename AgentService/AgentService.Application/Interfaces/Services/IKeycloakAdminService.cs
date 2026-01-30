using AgentService.Domain.Entities;

namespace AgentService.Application.Interfaces.Services;

public interface IKeycloakAdminService
{
    Task<bool> UserExistsAsync(string email);
    Task<string> CreateUserAsync(User user, string temporaryPassword);
}
