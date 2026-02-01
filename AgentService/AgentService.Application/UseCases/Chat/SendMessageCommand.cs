using MediatR;
using AgentService.Application.DTOs.Chat;

namespace AgentService.Application.UseCases.Chat;

public record SendMessageCommand(
    Guid? ConversationId,
    string Message,
    string? SessionId,
    string Platform,
    Guid? UserId,
    Guid? AgentId = null,
    string Model = "Gemini"
) : IRequest<SendMessageResponse>;
