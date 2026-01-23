using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AgentService.Domain.Entities.Auth;

namespace AgentService.Infrastructure.Data.Configurations.Auth;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(x => x.Code); // Code is PK
        builder.Property(x => x.Code).HasMaxLength(100);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(250);
        builder.Property(x => x.Group).IsRequired().HasMaxLength(50);
    }
}
