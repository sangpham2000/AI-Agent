using MediatR;

namespace AgentService.Application.UseCases.Users.Commands;

public record UpdateUserCommand(Guid Id, string Username, string Email, string FirstName, string LastName, string? PhoneNumber, DateTime? DateOfBirth, string? AvatarUrl, bool IsActive) : IRequest<bool>;


