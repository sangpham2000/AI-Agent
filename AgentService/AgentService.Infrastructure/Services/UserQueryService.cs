using Mapster;
using Microsoft.EntityFrameworkCore;
using AgentService.Application.DTOs;
using AgentService.Application.Interfaces.Services;
using AgentService.Infrastructure.Data;

namespace AgentService.Infrastructure.Services;

public class UserQueryService : IUserQueryService
{
    private readonly ApplicationReadDbContext _context;

    public UserQueryService(ApplicationReadDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .ProjectToType<UserDto>()
            .ToListAsync();
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .ProjectToType<UserDto>()
            .FirstOrDefaultAsync();
    }
}
