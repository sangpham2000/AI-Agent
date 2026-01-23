namespace AgentService.Application.DTOs.Chat;

/// <summary>
/// Response for admin listing all conversations
/// </summary>
public record AdminListConversationsResponse(
    IEnumerable<ConversationSummaryDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);

/// <summary>
/// Summary of a conversation for listing
/// </summary>
public record ConversationSummaryDto(
    Guid Id,
    string? UserId,
    string? UserName,
    string? UserEmail,
    string Platform,
    int MessageCount,
    DateTime StartedAt,
    DateTime? EndedAt,
    string Status
);

/// <summary>
/// Export result
/// </summary>
public record ExportConversationResult(
    byte[] Data,
    string FileName
);
