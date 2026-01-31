using Microsoft.EntityFrameworkCore;
using AgentService.Application.Interfaces.Services;
using AgentService.Application.UseCases.Chat; // For IApplicationDbContext
using AgentService.Domain.Entities;

namespace AgentService.Infrastructure.Services;

public class QuotaService : IQuotaService
{
    private readonly IApplicationDbContext _context;
    private const long DEFAULT_MONTHLY_LIMIT = 100_000;

    public QuotaService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CheckQuotaAsync(Guid userId)
    {
        var quota = await GetOrCreateQuotaAsync(userId);
        
        // Reset if new month
        if (quota.LastResetDate < DateTime.UtcNow.AddMonths(-1))
        {
            quota.UsedTokens = 0;
            quota.LastResetDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return quota.UsedTokens < quota.MonthlyTokenLimit;
    }

    public async Task ConsumeQuotaAsync(Guid userId, int tokens)
    {
        var quota = await GetOrCreateQuotaAsync(userId);
        
        quota.UsedTokens += tokens;
        
        // Ensure reset logic applies here too just in case
        if (quota.LastResetDate < DateTime.UtcNow.AddMonths(-1))
        {
            quota.UsedTokens = tokens; // New month starts with this usage
            quota.LastResetDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<UserQuota> GetOrCreateQuotaAsync(Guid userId)
    {
        var quota = await _context.UserQuotas
            .FirstOrDefaultAsync(q => q.UserId == userId);

        if (quota == null)
        {
            quota = new UserQuota
            {
                UserId = userId,
                MonthlyTokenLimit = DEFAULT_MONTHLY_LIMIT,
                UsedTokens = 0,
                LastResetDate = DateTime.UtcNow
            };
            _context.UserQuotas.Add(quota);
            await _context.SaveChangesAsync();
        }

        return quota;
    }
}
