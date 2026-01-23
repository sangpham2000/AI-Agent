using AgentService.Application.DTOs.Analytics;

namespace AgentService.Application.Interfaces.Services;

/// <summary>
/// Service for analytics and dashboard statistics
/// </summary>
public interface IAnalyticsService
{
    /// <summary>
    /// Get dashboard statistics summary
    /// </summary>
    Task<DashboardStatsDto> GetDashboardStatsAsync();
    
    /// <summary>
    /// Get conversation trends over time
    /// </summary>
    Task<IEnumerable<ConversationTrendDto>> GetConversationTrendsAsync(int days = 7);
    
    /// <summary>
    /// Get popular questions asked
    /// </summary>
    Task<IEnumerable<PopularQuestionDto>> GetPopularQuestionsAsync(int limit = 10);
    
    /// <summary>
    /// Get daily message counts
    /// </summary>
    Task<IEnumerable<DailyMessageCountDto>> GetDailyMessageCountsAsync(int days = 7);
    
    /// <summary>
    /// Get platform distribution
    /// </summary>
    Task<IEnumerable<PlatformDistributionDto>> GetPlatformDistributionAsync();
}
