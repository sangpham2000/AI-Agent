using MediatR;
using AgentService.Application.DTOs.Chat;

namespace AgentService.Application.UseCases.Chat;

public record GetConversationQuery(Guid ConversationId, Guid? UserId) : IRequest<ConversationDetailDto?>;

public record ListConversationsQuery(Guid? UserId, string? SessionId, int Page = 1, int PageSize = 20) 
    : IRequest<ListConversationsResponse>;

public record DeleteConversationCommand(Guid ConversationId, Guid? UserId) : IRequest<bool>;
