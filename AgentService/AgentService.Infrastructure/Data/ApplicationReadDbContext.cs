using Microsoft.EntityFrameworkCore;
using AgentService.Domain.Entities;
using AgentService.Domain.Entities.Auth;

namespace AgentService.Infrastructure.Data;

public class ApplicationReadDbContext : ApplicationDbContext
{
    public ApplicationReadDbContext(DbContextOptions<ApplicationReadDbContext> options) : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("This context is read-only.");
    }

    public override int SaveChanges()
    {
        throw new InvalidOperationException("This context is read-only.");
    }
}
