using Mapster;
using MediatR;
using AgentService.Application.DTOs;
using AgentService.Application.Interfaces.Repositories;
using AgentService.Application.Interfaces.Services;
using AgentService.Domain.Entities;

namespace AgentService.Application.UseCases.Users.Commands.Handlers;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IUserWriteRepository _userWriteRepository;
    private readonly IKeycloakAdminService _keycloakAdminService;

    public CreateUserCommandHandler(IUserWriteRepository userWriteRepository, IKeycloakAdminService keycloakAdminService)
    {
        _userWriteRepository = userWriteRepository;
        _keycloakAdminService = keycloakAdminService;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Check if user exists in Keycloak
        if (await _keycloakAdminService.UserExistsAsync(request.Email))
        {
            throw new Exception($"User with email {request.Email} already exists in Keycloak.");
        }

        var user = request.Adapt<User>();
        
        // 2. Create user in Keycloak
        // TODO: Generate a random password or receive it from request? 
        // For now using a default strong password for initial setup.
        var tempPassword = "Password@123"; 
        var keycloakId = await _keycloakAdminService.CreateUserAsync(user, tempPassword);
        
        // 3. Sync ID with Keycloak (Best Practice)
        if (Guid.TryParse(keycloakId, out var parsedId))
        {
            user.Id = parsedId;
        }
        else
        {
            // Fallback if Keycloak ID is not a GUID (rare but possible in some configs)
            user.Id = Guid.NewGuid();
        }

        user.IsActive = true;
        user.CreatedAt = DateTime.UtcNow;

        await _userWriteRepository.AddAsync(user);

        return user.Adapt<UserDto>();
    }
}
