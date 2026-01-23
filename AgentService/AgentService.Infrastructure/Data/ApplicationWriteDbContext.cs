using Microsoft.EntityFrameworkCore;
using AgentService.Application.UseCases.Chat;
using AgentService.Domain.Entities;

namespace AgentService.Infrastructure.Data;

public class ApplicationWriteDbContext : ApplicationDbContext, IApplicationDbContext
{
    public ApplicationWriteDbContext(DbContextOptions<ApplicationWriteDbContext> options) : base(options)
    {
    }
}
