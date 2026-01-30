using MediatR;
using AgentService.Application.DTOs;

namespace AgentService.Application.UseCases.Roles.Queries;

public record GetAllRolesQuery : IRequest<IEnumerable<RoleDto>>;
