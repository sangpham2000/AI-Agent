using AgentService.Domain.Entities;
using AgentService.Domain.Entities.Auth;

namespace AgentService.Application.Interfaces.Repositories;

public interface IUserWriteRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid id);
    Task<Role?> GetRoleByNameAsync(string roleName);
    Task AddRoleAsync(Role role);
}
