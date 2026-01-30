using AgentService.Application.Interfaces.Repositories;
using AgentService.Domain.Entities;
using AgentService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

using AgentService.Domain.Entities.Auth;

namespace AgentService.Infrastructure.Repositories;

public class UserWriteRepository : IUserWriteRepository
{
    private readonly ApplicationWriteDbContext _context;

    public UserWriteRepository(ApplicationWriteDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Role?> GetRoleByNameAsync(string roleName)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
    }

    public async Task AddRoleAsync(Role role)
    {
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
