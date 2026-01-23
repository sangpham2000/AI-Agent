namespace AgentService.Application.DTOs.Chat;

public record SendMessageRequest(
    Guid? ConversationId,
    string Message,
    string? SessionId = null,
    string Platform = "web_plugin"
);

public record SendMessageResponse(
    Guid ConversationId,
    Guid MessageId,
    string Response,
    string? Metadata = null
);

public record ConversationDto(
    Guid Id,
    string? Title,
    string Platform,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int MessageCount
);

public record MessageDto(
    Guid Id,
    string Role,
    string Content,
    DateTime CreatedAt,
    string? Metadata = null
);

public record ConversationDetailDto(
    Guid Id,
    string? UserId,
    string? SessionId,
    string Platform,
    List<MessageDto> Messages,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record ListConversationsRequest(
    int Page = 1,
    int PageSize = 20
);

public record ListConversationsResponse(
    List<ConversationDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);
