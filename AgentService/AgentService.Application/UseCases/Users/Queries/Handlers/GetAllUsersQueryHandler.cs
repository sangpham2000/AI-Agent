using MediatR;
using AgentService.Application.DTOs;
using AgentService.Application.Interfaces.Services;

namespace AgentService.Application.UseCases.Users.Queries.Handlers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserDto>>
{
    private readonly IUserQueryService _userQueryService;

    public GetAllUsersQueryHandler(IUserQueryService userQueryService)
    {
        _userQueryService = userQueryService;
    }

    public async Task<IEnumerable<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        return await _userQueryService.GetAllAsync();
    }
}
