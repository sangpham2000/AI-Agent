using Mapster;
using MediatR;
using AgentService.Application.DTOs;
using AgentService.Application.Interfaces.Repositories;

namespace AgentService.Application.UseCases.Roles.Queries.Handlers;

public class GetAllPermissionsQueryHandler : IRequestHandler<GetAllPermissionsQuery, IEnumerable<PermissionDto>>
{
    private readonly IRoleWriteRepository _roleValues; // Utilizing existing Write Repo for read to avoid complexity

    public GetAllPermissionsQueryHandler(IRoleWriteRepository roleValues)
    {
        _roleValues = roleValues;
    }

    public async Task<IEnumerable<PermissionDto>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await _roleValues.GetAllPermissionsAsync();
        return permissions.Adapt<IEnumerable<PermissionDto>>();
    }
}
