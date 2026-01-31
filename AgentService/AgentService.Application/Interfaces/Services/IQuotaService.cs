using AgentService.Domain.Entities;

namespace AgentService.Application.Interfaces.Services;

public interface IQuotaService
{
    Task<bool> CheckQuotaAsync(Guid userId);
    Task ConsumeQuotaAsync(Guid userId, int tokens);
    Task<UserQuota> GetOrCreateQuotaAsync(Guid userId);
}
