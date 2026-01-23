using MediatR;
using AgentService.Application.DTOs.Chat;

namespace AgentService.Application.UseCases.Conversations;

/// <summary>
/// Query to list all conversations for admin
/// </summary>
public record AdminListConversationsQuery(
    string? Platform,
    string? Search,
    DateTime? StartDate,
    DateTime? EndDate,
    int Page,
    int PageSize
) : IRequest<AdminListConversationsResponse>;

/// <summary>
/// Query to get a conversation by ID for admin
/// </summary>
public record AdminGetConversationQuery(Guid ConversationId) : IRequest<ConversationDetailDto?>;

/// <summary>
/// Query to export a conversation
/// </summary>
public record ExportConversationQuery(Guid ConversationId, string Format) : IRequest<ExportConversationResult?>;

/// <summary>
/// Command to delete a conversation (admin)
/// </summary>
public record AdminDeleteConversationCommand(Guid ConversationId) : IRequest<bool>;
