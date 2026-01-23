namespace AgentService.Application.DTOs;

public record UserDto(Guid Id, string Username, string Email, string FirstName, string LastName, string? PhoneNumber, DateTime? DateOfBirth, string? AvatarUrl, bool IsActive, DateTime? LastLoginAt, DateTime CreatedAt);
public record CreateUserDto(string Username, string Email, string FirstName, string LastName, string? PhoneNumber, DateTime? DateOfBirth, string? AvatarUrl);
public record UpdateUserDto(string Username, string Email, string FirstName, string LastName, string? PhoneNumber, DateTime? DateOfBirth, string? AvatarUrl, bool IsActive);
