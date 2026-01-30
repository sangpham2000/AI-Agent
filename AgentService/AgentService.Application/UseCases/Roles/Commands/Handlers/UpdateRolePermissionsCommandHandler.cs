using MediatR;
using AgentService.Application.Interfaces.Repositories;

namespace AgentService.Application.UseCases.Roles.Commands.Handlers;

public class UpdateRolePermissionsCommandHandler : IRequestHandler<UpdateRolePermissionsCommand, bool>
{
    private readonly IRoleWriteRepository _roleWriteRepository;

    public UpdateRolePermissionsCommandHandler(IRoleWriteRepository roleWriteRepository)
    {
        _roleWriteRepository = roleWriteRepository;
    }

    public async Task<bool> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleWriteRepository.GetByIdAsync(request.RoleId);
        if (role == null) return false;

        await _roleWriteRepository.UpdateRolePermissionsAsync(request.RoleId, request.PermissionIds);
        return true;
    }
}
