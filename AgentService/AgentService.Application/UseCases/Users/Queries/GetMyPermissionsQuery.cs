using MediatR;
using AgentService.Application.DTOs;

namespace AgentService.Application.UseCases.Users.Queries;

public record GetMyPermissionsQuery(Guid UserId) : IRequest<UserPermissionsDto>;
