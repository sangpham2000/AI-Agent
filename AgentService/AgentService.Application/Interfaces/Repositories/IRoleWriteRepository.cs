using AgentService.Domain.Entities.Auth;

namespace AgentService.Application.Interfaces.Repositories;

public interface IRoleWriteRepository
{
    Task<Role?> GetByIdAsync(Guid id);
    Task UpdateAsync(Role role);
    Task<IEnumerable<Permission>> GetAllPermissionsAsync();
    Task UpdateRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds);
}
