using Mapster;
using MediatR;
using AgentService.Application.Interfaces.Repositories;
using AgentService.Application.Interfaces.Services;

namespace AgentService.Application.UseCases.Users.Commands.Handlers;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, bool>
{
    private readonly IUserQueryService _userQueryService;
    private readonly IUserWriteRepository _userWriteRepository;

    public UpdateUserCommandHandler(IUserQueryService userQueryService, IUserWriteRepository userWriteRepository)
    {
        _userQueryService = userQueryService;
        _userWriteRepository = userWriteRepository;
    }

    public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userWriteRepository.GetByIdAsync(request.Id);
        if (user == null) return false;

        request.Adapt(user); // Map properties from command to existing user entity

        await _userWriteRepository.UpdateAsync(user);
        return true;
    }
}
