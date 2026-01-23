using MediatR;
using AgentService.Application.Interfaces.Repositories;

namespace AgentService.Application.UseCases.Users.Commands.Handlers;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserWriteRepository _userWriteRepository;

    public DeleteUserCommandHandler(IUserWriteRepository userWriteRepository)
    {
        _userWriteRepository = userWriteRepository;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        await _userWriteRepository.DeleteAsync(request.Id);
    }
}
