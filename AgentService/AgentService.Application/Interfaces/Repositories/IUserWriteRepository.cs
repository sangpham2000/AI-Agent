using AgentService.Domain.Entities;

namespace AgentService.Application.Interfaces.Repositories;

public interface IUserWriteRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid id);
}
