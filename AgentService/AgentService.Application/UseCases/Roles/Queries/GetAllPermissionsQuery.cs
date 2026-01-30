using MediatR;
using AgentService.Application.DTOs;

namespace AgentService.Application.UseCases.Roles.Queries;

public record GetAllPermissionsQuery : IRequest<IEnumerable<PermissionDto>>;
