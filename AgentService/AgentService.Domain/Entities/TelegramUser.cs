using System.ComponentModel.DataAnnotations;

namespace AgentService.Domain.Entities;

public class TelegramUser
{
    [Key]
    public long ChatId { get; set; }
    
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    
    public string Username { get; set; } = string.Empty;
    
    public RegistrationState State { get; set; } = RegistrationState.None;
    
    public UserType TempRole { get; set; } = UserType.Unknown;
}

public enum RegistrationState
{
    None,
    SelectingRole,
    EnteringStudentId,
    Completed
}
