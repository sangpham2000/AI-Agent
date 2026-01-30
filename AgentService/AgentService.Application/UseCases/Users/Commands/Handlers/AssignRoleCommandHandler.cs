using MediatR;
using AgentService.Application.Interfaces.Repositories;
using AgentService.Domain.Entities.Auth;

namespace AgentService.Application.UseCases.Users.Commands.Handlers;

public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, bool>
{
    private readonly IUserWriteRepository _userWriteRepository;

    public AssignRoleCommandHandler(IUserWriteRepository userWriteRepository)
    {
        _userWriteRepository = userWriteRepository;
    }

    public async Task<bool> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userWriteRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            return false;
        }

        var role = await _userWriteRepository.GetRoleByNameAsync(request.RoleName);
        if (role == null)
        {
            // Auto-create role if it doesn't exist (e.g. SuperAdmin)
            role = new Role
            {
                Id = Guid.NewGuid(),
                Name = request.RoleName,
                Description = $"Auto-created role {request.RoleName}",
                IsSystemRole = false,
                CreatedAt = DateTime.UtcNow
            };
            
            await _userWriteRepository.AddRoleAsync(role);
        }

        // Check if user already has this role
        if (!user.UserRoles.Any(ur => ur.RoleId == role.Id))
        {
            user.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            });
            await _userWriteRepository.UpdateAsync(user);
        }

        return true;
    }
}
