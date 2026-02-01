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
        // Messages per day (last 7 days)
        // Fetch timestamps first to perform reliable grouping in-memory (avoids TimeZone/EF translation issues)
        var messageTimestamps = await _context.Messages
            .Where(m => m.CreatedAt >= weekAgo)
            .Select(m => m.CreatedAt)
            .ToListAsync();
            
        var messagesThisWeek = new int[7];
        for (int i = 0; i < 7; i++)
        {
            var date = weekAgo.AddDays(i);
            // Compare Date components locally
            messagesThisWeek[i] = messageTimestamps.Count(t => t.Date == date);
        }

        // Platform distribution
        var platformDistribution = await _context.Conversations
            .GroupBy(c => c.Platform)
            .Select(g => new PlatformDistributionDto(g.Key ?? "Unknown", g.Count()))
            .ToArrayAsync();

        // Total Token Usage (Sum of UsedTokens from all UserQuotas)
        var totalTokensUsed = await _context.UserQuotas.SumAsync(q => q.UsedTokens);

        // Recent Activity Aggregation
        var recentUsers = await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
            .Select(u => new RecentActivityDto(
                u.Id.ToString(),
                $"New user registered: {u.Username}",
                "User",
                "New",
                u.CreatedAt
            ))
            .ToListAsync();

        var recentDocs = await _context.Documents
            .OrderByDescending(d => d.CreatedAt)
            .Take(5)
            .Select(d => new RecentActivityDto(
                d.Id.ToString(),
                $"Document uploaded: {d.Title ?? d.FileName}",
                "Document",
                d.IsProcessed ? "Processed" : "Pending",
                d.CreatedAt
            ))
            .ToListAsync();

        var recentChats = await _context.Conversations
            .OrderByDescending(c => c.CreatedAt)
            .Take(5)
            .Select(c => new RecentActivityDto(
                c.Id.ToString(),
                $"Chat started via {c.Platform}",
                "Chat",
                c.IsActive ? "Active" : "Closed",
                c.CreatedAt
            ))
            .ToListAsync();

        var allActivities = recentUsers
            .Concat(recentDocs)
            .Concat(recentChats)
            .OrderByDescending(x => x.Timestamp)
            .Take(10)
            .ToList();


        // --- Growth Calculation (This Month vs Last Month) ---
        var firstDayThisMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var firstDayLastMonth = firstDayThisMonth.AddMonths(-1);
        
        var conversationsThisMonth = await _context.Conversations.CountAsync(c => c.CreatedAt >= firstDayThisMonth);
        var conversationsLastMonth = await _context.Conversations.CountAsync(c => c.CreatedAt >= firstDayLastMonth && c.CreatedAt < firstDayThisMonth);
        
        double conversationGrowthRate = 0;
        if (conversationsLastMonth > 0)
        {
            conversationGrowthRate = ((double)(conversationsThisMonth - conversationsLastMonth) / conversationsLastMonth) * 100;
        }
        else if (conversationsThisMonth > 0)
        {
            conversationGrowthRate = 100; // 100% growth if started from 0
        }

        double tokenUseGrowthRate = 0;

        // --- AI Performance (Avg Tokens / Assistant Message) ---
        var totalAssistantMessages = await _context.Messages.CountAsync(m => m.Role == "assistant");
        double avgTokensPerResponse = 0;
        if (totalAssistantMessages > 0)
        {
            avgTokensPerResponse = (double)totalTokensUsed / totalAssistantMessages;
        }

        // --- Active Models (Simulated/Config based) ---
        var activeModels = new List<ActiveModelDto>
        {
            new ActiveModelDto("Gemini 1.5 Flash", "Active", true),
            new ActiveModelDto("GPT-4o Mini", "Inactive", false)
        };

        return new DashboardStatsDto(
            totalConversations,
            conversationsToday,
            totalDocuments,
            documentsProcessed,
            totalUsers,
            activeUsers,
            totalTokensUsed,
            messagesThisWeek,
            platformDistribution,
            allActivities,
            Math.Round(conversationGrowthRate, 1),
            tokenUseGrowthRate,
            Math.Round(avgTokensPerResponse, 0),
            activeModels
        );
    }

    public async Task<IEnumerable<ConversationTrendDto>> GetConversationTrendsAsync(int days = 7)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days + 1);
        var previousStartDate = startDate.AddDays(-days);

        // Fetch timestamps for both periods to perform reliable grouping in-memory
        var timestamps = await _context.Conversations
            .Where(c => c.CreatedAt >= previousStartDate)
            .Select(c => c.CreatedAt)
            .ToListAsync();

        var result = new List<ConversationTrendDto>();
        for (int i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);
            var previousDate = date.AddDays(-days);
            
            var count = timestamps.Count(t => t.Date == date);
            var previousCount = timestamps.Count(t => t.Date == previousDate);
            
            result.Add(new ConversationTrendDto(date, count, previousCount));
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

        // Fetch timestamps to perform reliable grouping in-memory
        var timestamps = await _context.Messages
            .Where(m => m.CreatedAt >= startDate)
            .Select(m => m.CreatedAt)
            .ToListAsync();
            
        var result = new List<DailyMessageCountDto>();
        for (int i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);
            var count = timestamps.Count(t => t.Date == date);
            result.Add(new DailyMessageCountDto(date, count));
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
