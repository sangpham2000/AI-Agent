using MediatR;
using AgentService.Application.DTOs;

namespace AgentService.Application.UseCases.Users.Commands;

public record CreateUserCommand(string Username, string Email, string FirstName, string LastName, string? PhoneNumber, DateTime? DateOfBirth, string? AvatarUrl) : IRequest<UserDto>;


