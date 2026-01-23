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
    int[] MessagesThisWeek,
    PlatformDistributionDto[] ConversationsByPlatform
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
