using MediatR;

namespace AgentService.Application.UseCases.Roles.Commands;

public record UpdateRolePermissionsCommand(Guid RoleId, IEnumerable<Guid> PermissionIds) : IRequest<bool>;
