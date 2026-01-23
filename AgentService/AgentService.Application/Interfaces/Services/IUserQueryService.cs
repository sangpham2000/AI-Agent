using AgentService.Application.DTOs;

namespace AgentService.Application.Interfaces.Services;

public interface IUserQueryService
{
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(Guid id);
}
