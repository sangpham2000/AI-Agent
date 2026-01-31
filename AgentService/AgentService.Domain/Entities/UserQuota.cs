using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgentService.Domain.Entities;

public class UserQuota
{
    [Key]
    public Guid UserId { get; set; }
    
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public long MonthlyTokenLimit { get; set; }
    public long UsedTokens { get; set; }
    public DateTime LastResetDate { get; set; }
}
