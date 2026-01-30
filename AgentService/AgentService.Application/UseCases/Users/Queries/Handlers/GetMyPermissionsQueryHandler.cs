using MediatR;
using AgentService.Application.DTOs;
using AgentService.Application.Interfaces.Services;

namespace AgentService.Application.UseCases.Users.Queries.Handlers;

public class GetMyPermissionsQueryHandler : IRequestHandler<GetMyPermissionsQuery, UserPermissionsDto>
{
    private readonly IUserQueryService _userQueryService;

    public GetMyPermissionsQueryHandler(IUserQueryService userQueryService)
    {
        _userQueryService = userQueryService;
    }

    public async Task<UserPermissionsDto> Handle(GetMyPermissionsQuery request, CancellationToken cancellationToken)
    {
        var user = await _userQueryService.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return new UserPermissionsDto(false, false, Enumerable.Empty<string>());
        }

        var roles = user.Roles;
        var isAdmin = roles.Contains("Admin") || roles.Contains("SuperAdmin");
        var isSuperAdmin = roles.Contains("SuperAdmin");

        return new UserPermissionsDto(isAdmin, isSuperAdmin, roles);
    }
}
