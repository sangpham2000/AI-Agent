using MediatR;
using AgentService.Application.DTOs;

namespace AgentService.Application.UseCases.Users.Queries;

public record GetMyQuotaQuery(Guid UserId) : IRequest<UserQuotaDto>;
