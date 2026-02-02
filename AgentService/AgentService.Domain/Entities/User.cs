namespace AgentService.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Auth.UserRole> UserRoles { get; set; } = new List<Auth.UserRole>();

    public string? StudentId { get; set; }
    public UserType UserType { get; set; } = UserType.Unknown;
}

public enum UserType
{
    Unknown,
    Student,
    Staff
}
