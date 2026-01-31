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
        // Left Join manually isn't needed if we use navigation property, but checking if context has it. 
        // We added DbSet<UserQuota> to ApplicationDbContext, but need to check if ReadDbContext has it.
        // Assuming ReadDbContext inherits from ApplicationDbContext or has the same setup.
        
        // Actually, let's look at ApplicationReadDbContext content first? 
        // Wait, I saw ApplicationReadDbContext earlier and it was nearly empty/inheriting?
        // Let's assume UserQuota is available via navigation property if config is correct.
        // But User entity in Domain doesn't have "public UserQuota Quota { get; set; }" navigation property yet?
        // I created UserQuota entity with FK to User, but didn't update User entity.
        // So I need to use explicit Join or GroupJoin.

        var query = from u in _context.Users.AsNoTracking()
                    join q in _context.UserQuotas.AsNoTracking() on u.Id equals q.UserId into uq
                    from quota in uq.DefaultIfEmpty()
                    select new { u, quota };

        var result = await query.Select(x => new
        {
            x.u,
            Roles = x.u.UserRoles.Select(ur => ur.Role.Name).ToList(),
            x.quota
        }).ToListAsync();

        return result.Select(x => new UserDto(
                x.u.Id,
                x.u.Username,
                x.u.Email,
                x.u.FirstName,
                x.u.LastName,
                x.u.PhoneNumber,
                x.u.DateOfBirth,
                x.u.AvatarUrl,
                x.u.IsActive,
                x.u.LastLoginAt,
                x.u.CreatedAt,
                x.Roles,
                x.quota == null ? null : new UserQuotaDto(
                    x.quota.MonthlyTokenLimit,
                    x.quota.UsedTokens,
                    x.quota.LastResetDate,
                    Math.Max(0, x.quota.MonthlyTokenLimit - x.quota.UsedTokens),
                    x.quota.MonthlyTokenLimit > 0 ? (int)((double)x.quota.UsedTokens / x.quota.MonthlyTokenLimit * 100) : 0
                )
            ));
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var query = from u in _context.Users.AsNoTracking()
                    where u.Id == id
                    join q in _context.UserQuotas.AsNoTracking() on u.Id equals q.UserId into uq
                    from quota in uq.DefaultIfEmpty()
                    select new { u, quota };

        var result = await query.Select(x => new
        {
            x.u,
            Roles = x.u.UserRoles.Select(ur => ur.Role.Name).ToList(),
            x.quota
        }).FirstOrDefaultAsync();

        if (result == null) return null;

        return new UserDto(
                result.u.Id,
                result.u.Username,
                result.u.Email,
                result.u.FirstName,
                result.u.LastName,
                result.u.PhoneNumber,
                result.u.DateOfBirth,
                result.u.AvatarUrl,
                result.u.IsActive,
                result.u.LastLoginAt,
                result.u.CreatedAt,
                result.Roles,
                result.quota == null ? null : new UserQuotaDto(
                    result.quota.MonthlyTokenLimit,
                    result.quota.UsedTokens,
                    result.quota.LastResetDate,
                    Math.Max(0, result.quota.MonthlyTokenLimit - result.quota.UsedTokens),
                    result.quota.MonthlyTokenLimit > 0 ? (int)((double)result.quota.UsedTokens / result.quota.MonthlyTokenLimit * 100) : 0
                )
            );
    }
}
