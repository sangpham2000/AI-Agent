using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgentService.Application.DTOs.Analytics;
using AgentService.Application.Interfaces.Services;

namespace AgentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Get dashboard statistics summary
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
    {
        var stats = await _analyticsService.GetDashboardStatsAsync();
        return Ok(stats);
    }

    /// <summary>
    /// Get conversation trends over time
    /// </summary>
    [HttpGet("conversations/trends")]
    public async Task<ActionResult<IEnumerable<ConversationTrendDto>>> GetConversationTrends(
        [FromQuery] int days = 7)
    {
        var trends = await _analyticsService.GetConversationTrendsAsync(days);
        return Ok(trends);
    }

    /// <summary>
    /// Get popular questions
    /// </summary>
    [HttpGet("questions/popular")]
    public async Task<ActionResult<IEnumerable<PopularQuestionDto>>> GetPopularQuestions(
        [FromQuery] int limit = 10)
    {
        var questions = await _analyticsService.GetPopularQuestionsAsync(limit);
        return Ok(questions);
    }

    /// <summary>
    /// Get daily message counts
    /// </summary>
    [HttpGet("messages/daily")]
    public async Task<ActionResult<IEnumerable<DailyMessageCountDto>>> GetDailyMessageCounts(
        [FromQuery] int days = 7)
    {
        var counts = await _analyticsService.GetDailyMessageCountsAsync(days);
        return Ok(counts);
    }

    /// <summary>
    /// Get platform distribution
    /// </summary>
    [HttpGet("platforms")]
    public async Task<ActionResult<IEnumerable<PlatformDistributionDto>>> GetPlatformDistribution()
    {
        var distribution = await _analyticsService.GetPlatformDistributionAsync();
        return Ok(distribution);
    }
}
