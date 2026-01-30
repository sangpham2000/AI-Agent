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
            .Select(u => new UserDto(
                u.Id,
                u.Username,
                u.Email,
                u.FirstName,
                u.LastName,
                u.PhoneNumber,
                u.DateOfBirth,
                u.AvatarUrl,
                u.IsActive,
                u.LastLoginAt,
                u.CreatedAt,
                u.UserRoles.Select(ur => ur.Role.Name).ToList()
            ))
            .ToListAsync();
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserDto(
                u.Id,
                u.Username,
                u.Email,
                u.FirstName,
                u.LastName,
                u.PhoneNumber,
                u.DateOfBirth,
                u.AvatarUrl,
                u.IsActive,
                u.LastLoginAt,
                u.CreatedAt,
                u.UserRoles.Select(ur => ur.Role.Name).ToList()
            ))
            .FirstOrDefaultAsync();
    }
}
