using AgentService.Application.Interfaces.Repositories;
using AgentService.Domain.Entities.Auth;
using AgentService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgentService.Infrastructure.Repositories;

public class RoleWriteRepository : IRoleWriteRepository
{
    private readonly ApplicationWriteDbContext _context;

    public RoleWriteRepository(ApplicationWriteDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByIdAsync(Guid id)
    {
        return await _context.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task UpdateAsync(Role role)
    {
        _context.Roles.Update(role);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
    {
        return await _context.Permissions.ToListAsync();
    }

    public async Task UpdateRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null) return;

        // Verify all permissions exist
        var validPermissionIds = await _context.Permissions
            .Where(p => permissionIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync();

        // Clear existing permissions
        role.RolePermissions.Clear();

        // Add new permissions
        foreach (var permissionId in validPermissionIds)
        {
            role.RolePermissions.Add(new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            });
        }

        await _context.SaveChangesAsync();
    }
}
