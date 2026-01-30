using AgentService.Application.DTOs;

namespace AgentService.Application.Interfaces.Services;

public interface IRoleQueryService
{
    Task<IEnumerable<RoleDto>> GetAllAsync();
}
