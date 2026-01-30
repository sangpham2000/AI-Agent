using Mapster;
using Microsoft.EntityFrameworkCore;
using AgentService.Application.DTOs;
using AgentService.Application.Interfaces.Services;
using AgentService.Infrastructure.Data;

namespace AgentService.Infrastructure.Services;

public class RoleQueryService : IRoleQueryService
{
    private readonly ApplicationReadDbContext _context;

    public RoleQueryService(ApplicationReadDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RoleDto>> GetAllAsync()
    {
        return await _context.Roles
            .AsNoTracking()
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .Select(r => new RoleDto(
                r.Id,
                r.Name, 
                r.Description,
                r.RolePermissions.Select(rp => new PermissionDto(
                    rp.Permission.Id,
                    rp.Permission.Code,
                    rp.Permission.Name,
                    rp.Permission.Description,
                    rp.Permission.Group
                )).ToList()
            ))
            .ToListAsync();
    }
}
