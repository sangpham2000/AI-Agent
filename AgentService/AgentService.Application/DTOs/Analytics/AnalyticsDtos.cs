namespace AgentService.Application.DTOs.Analytics;

/// <summary>
/// Dashboard statistics summary
/// </summary>
public record DashboardStatsDto(
    int TotalConversations,
    int ConversationsToday,
    int TotalDocuments,
    int DocumentsProcessed,
    int TotalUsers,
    int ActiveUsers,
    long TotalTokensUsedThisMonth,
    int[] MessagesThisWeek,
    PlatformDistributionDto[] ConversationsByPlatform,
    IEnumerable<RecentActivityDto> RecentActivities,
    double ConversationGrowthRate,
    double TokenUseGrowthRate,
    double AvgTokensPerResponse,
    IEnumerable<ActiveModelDto> ActiveModels
);

/// <summary>
/// Active AI Model info
/// </summary>
public record ActiveModelDto(
    string Name,
    string Status, // "Active", "Inactive"
    bool IsDefault
);

/// <summary>
/// Recent system activity
/// </summary>
public record RecentActivityDto(
    string Id,
    string Description,
    string Type, 
    string Status, 
    DateTime Timestamp
);

/// <summary>
/// Platform distribution for conversations
/// </summary>
public record PlatformDistributionDto(
    string Platform,
    int Count
);

/// <summary>
/// Conversation trend data point
/// </summary>
public record ConversationTrendDto(
    DateTime Date,
    int Count
);

/// <summary>
/// Popular question entry
/// </summary>
public record PopularQuestionDto(
    string Question,
    int Count,
    DateTime LastAsked
);

/// <summary>
/// Daily message count
/// </summary>
public record DailyMessageCountDto(
    DateTime Date,
    int Count
);
