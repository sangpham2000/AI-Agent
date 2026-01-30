using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using AgentService.Domain.Entities;
using AgentService.Domain.Entities.Auth;

namespace AgentService.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    
    // AI Agent Entities
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentChunk> DocumentChunks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply all IEntityTypeConfiguration
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        // Manual Configuration (Migrated from WriteDbContext)
        ConfigureEntities(modelBuilder);
        
        // Apply Snake Case Naming Convention
        ApplySnakeCaseNamingConvention(modelBuilder);
    }

    private void ConfigureEntities(ModelBuilder modelBuilder)
    {
        // UserRole - composite key
        modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
        
        // Permission
        modelBuilder.Entity<Permission>(entity => {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.Code).IsUnique();
        });
        
        // RolePermission - composite key & relationship fix
        modelBuilder.Entity<RolePermission>(entity => {
             entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });
             
             entity.HasOne(rp => rp.Permission)
                   .WithMany()
                   .HasForeignKey(rp => rp.PermissionId);
        });

        // Conversation
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.TelegramChatId);
            entity.HasIndex(e => e.SessionId);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        // Message
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ConversationId);
            entity.HasOne(e => e.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Document
        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("uploaded_files");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Category);
            entity.HasOne(e => e.UploadedBy)
                .WithMany()
                .HasForeignKey(e => e.UploadedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        // DocumentChunk
        modelBuilder.Entity<DocumentChunk>(entity =>
        {
            entity.ToTable("uploaded_file_chunks");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DocumentId);
            entity.HasOne(e => e.Document)
                .WithMany(d => d.Chunks)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ApplySnakeCaseNamingConvention(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Table names
            var tableName = entity.GetTableName();
            if (!string.IsNullOrEmpty(tableName))
            {
                 entity.SetTableName(ToSnakeCase(tableName));
            }

            // Column names
            foreach (var property in entity.GetProperties())
            {
                // Skip navigation properties (handled by EF), only primitive columns
                var storeObjectIdentifier = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
                property.SetColumnName(ToSnakeCase(property.GetColumnName(storeObjectIdentifier)));
            }

            // Keys and Indexes names
            foreach (var key in entity.GetKeys())
            {
                key.SetName(ToSnakeCase(key.GetName()));
            }

            foreach (var key in entity.GetForeignKeys())
            {
                key.SetConstraintName(ToSnakeCase(key.GetConstraintName()));
            }

            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName()));
            }
        }
    }

    private string? ToSnakeCase(string? input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        
        var startUnderscores = Regex.Match(input, @"^_+");
        return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
    }
}
