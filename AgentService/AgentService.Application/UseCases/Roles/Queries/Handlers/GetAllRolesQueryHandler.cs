using MediatR;
using AgentService.Application.DTOs;
using AgentService.Application.Interfaces.Services;

namespace AgentService.Application.UseCases.Roles.Queries.Handlers;

public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, IEnumerable<RoleDto>>
{
    private readonly IRoleQueryService _roleQueryService;

    public GetAllRolesQueryHandler(IRoleQueryService roleQueryService)
    {
        _roleQueryService = roleQueryService;
    }

    public async Task<IEnumerable<RoleDto>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        return await _roleQueryService.GetAllAsync();
    }
}
