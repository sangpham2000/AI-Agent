using Mapster;
using MediatR;
using AgentService.Application.DTOs;
using AgentService.Application.Interfaces.Repositories;
using AgentService.Domain.Entities;

namespace AgentService.Application.UseCases.Users.Commands.Handlers;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IUserWriteRepository _userWriteRepository;

    public CreateUserCommandHandler(IUserWriteRepository userWriteRepository)
    {
        _userWriteRepository = userWriteRepository;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = request.Adapt<User>();
        user.Id = Guid.NewGuid();
        user.IsActive = true;
        user.CreatedAt = DateTime.UtcNow;

        await _userWriteRepository.AddAsync(user);

        return user.Adapt<UserDto>();
    }
}
