using MediatR;
using AgentService.Application.DTOs;
using AgentService.Application.Interfaces.Services;

namespace AgentService.Application.UseCases.Users.Queries.Handlers;

public class GetMyQuotaQueryHandler : IRequestHandler<GetMyQuotaQuery, UserQuotaDto>
{
    private readonly IQuotaService _quotaService;

    public GetMyQuotaQueryHandler(IQuotaService quotaService)
    {
        _quotaService = quotaService;
    }

    public async Task<UserQuotaDto> Handle(GetMyQuotaQuery request, CancellationToken cancellationToken)
    {
        var quota = await _quotaService.GetOrCreateQuotaAsync(request.UserId);
        
        long remaining = quota.MonthlyTokenLimit - quota.UsedTokens;
        if (remaining < 0) remaining = 0;
        
        int percentage = 0;
        if (quota.MonthlyTokenLimit > 0)
        {
            percentage = (int)((double)quota.UsedTokens / quota.MonthlyTokenLimit * 100);
            if (percentage > 100) percentage = 100;
        }

        return new UserQuotaDto(
            quota.MonthlyTokenLimit,
            quota.UsedTokens,
            quota.LastResetDate,
            remaining,
            percentage
        );
    }
}
