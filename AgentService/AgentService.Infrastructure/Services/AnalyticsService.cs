using Microsoft.EntityFrameworkCore;
using AgentService.Application.DTOs.Analytics;
using AgentService.Application.Interfaces.Services;
using AgentService.Infrastructure.Data;

namespace AgentService.Infrastructure.Services;

/// <summary>
/// Implementation of analytics service
/// </summary>
public class AnalyticsService : IAnalyticsService
{
    private readonly ApplicationReadDbContext _context;

    public AnalyticsService(ApplicationReadDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var weekAgo = today.AddDays(-6);

        // Get counts
        var totalConversations = await _context.Conversations.CountAsync();
        var conversationsToday = await _context.Conversations
            .CountAsync(c => c.CreatedAt.Date == today);
        
        var totalDocuments = await _context.Documents.CountAsync();
        var documentsProcessed = await _context.Documents
            .CountAsync(d => d.IsProcessed);
        
        var totalUsers = await _context.Users.CountAsync();
        var activeUsers = await _context.Users
            .CountAsync(u => u.IsActive);

        // Messages per day (last 7 days)
        var messagesPerDay = await _context.Messages
            .Where(m => m.CreatedAt.Date >= weekAgo)
            .GroupBy(m => m.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync();

        // Fill in missing days with 0
        var messagesThisWeek = new int[7];
        for (int i = 0; i < 7; i++)
        {
            var date = weekAgo.AddDays(i);
            var dayData = messagesPerDay.FirstOrDefault(m => m.Date == date);
            messagesThisWeek[i] = dayData?.Count ?? 0;
        }

        // Platform distribution
        var platformDistribution = await _context.Conversations
            .GroupBy(c => c.Platform)
            .Select(g => new PlatformDistributionDto(g.Key ?? "Unknown", g.Count()))
            .ToArrayAsync();

        return new DashboardStatsDto(
            totalConversations,
            conversationsToday,
            totalDocuments,
            documentsProcessed,
            totalUsers,
            activeUsers,
            messagesThisWeek,
            platformDistribution
        );
    }

    public async Task<IEnumerable<ConversationTrendDto>> GetConversationTrendsAsync(int days = 7)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days + 1);

        // Use anonymous type for projection to ensure EF Core translation
        var rawTrends = await _context.Conversations
            .Where(c => c.CreatedAt >= startDate)
            .GroupBy(c => c.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(t => t.Date)
            .ToListAsync();

        var trends = rawTrends.Select(t => new ConversationTrendDto(t.Date, t.Count)).ToList();

        // Fill in missing days
        var result = new List<ConversationTrendDto>();
        for (int i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);
            var existing = trends.FirstOrDefault(t => t.Date == date);
            result.Add(existing ?? new ConversationTrendDto(date, 0));
        }

        return result;
    }

    public async Task<IEnumerable<PopularQuestionDto>> GetPopularQuestionsAsync(int limit = 10)
    {
        // Get most common user messages
        // Fix: Math.Min and complex Substring are not translatable to SQL in all providers
        // We fetch potential candidates first (users messages > 10 chars), then group in memory
        // We limit the fetch to recent/reasonable amount to avoid loading full DB
        
        var recentMessages = await _context.Messages
            .Where(m => m.Role == "user" && m.Content.Length > 10)
            .OrderByDescending(m => m.CreatedAt)
            .Take(2000) // Limit to analyze recently for performance
            .Select(m => new { m.Content, m.CreatedAt })
            .ToListAsync();

        var questions = recentMessages
            .GroupBy(m => m.Content.Substring(0, Math.Min(100, m.Content.Length)))
            .Select(g => new PopularQuestionDto(
                g.Key,
                g.Count(),
                g.Max(m => m.CreatedAt)
            ))
            .OrderByDescending(q => q.Count)
            .Take(limit)
            .ToList();

        return questions;
    }

    public async Task<IEnumerable<DailyMessageCountDto>> GetDailyMessageCountsAsync(int days = 7)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days + 1);

        // Use anonymous type with anonymous projection
        var rawCounts = await _context.Messages
            .Where(m => m.CreatedAt >= startDate)
            .GroupBy(m => m.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(c => c.Date)
            .ToListAsync();
            
        var counts = rawCounts.Select(c => new DailyMessageCountDto(c.Date, c.Count)).ToList();

        // Fill in missing days
        var result = new List<DailyMessageCountDto>();
        for (int i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);
            var existing = counts.FirstOrDefault(c => c.Date == date);
            result.Add(existing ?? new DailyMessageCountDto(date, 0));
        }

        return result;
    }

    public async Task<IEnumerable<PlatformDistributionDto>> GetPlatformDistributionAsync()
    {
        var distribution = await _context.Conversations
            .GroupBy(c => c.Platform)
            .Select(g => new PlatformDistributionDto(g.Key ?? "Unknown", g.Count()))
            .ToListAsync();

        return distribution;
    }
}
