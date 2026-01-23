using MediatR;
using AgentService.Application.DTOs;
using AgentService.Application.Interfaces.Services;

namespace AgentService.Application.UseCases.Users.Queries.Handlers;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserQueryService _userQueryService;

    public GetUserByIdQueryHandler(IUserQueryService userQueryService)
    {
        _userQueryService = userQueryService;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        return await _userQueryService.GetByIdAsync(request.Id);
    }
}
