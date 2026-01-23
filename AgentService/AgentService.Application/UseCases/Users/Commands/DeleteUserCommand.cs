using MediatR;

namespace AgentService.Application.UseCases.Users.Commands;

public record DeleteUserCommand(Guid Id) : IRequest;


