using MediatR;

namespace AgentService.Application.UseCases.Users.Commands;

public record AssignRoleCommand(Guid UserId, string RoleName) : IRequest<bool>;
