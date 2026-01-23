using AgentService.Domain.Entities.Auth;

namespace AgentService.Infrastructure.Data.Seeders;

public static class PermissionSeeder
{
    public static async Task SeedAsync(ApplicationWriteDbContext context)
    {
        if (!context.Permissions.Any())
        {
            var permissions = new List<Permission>
            {
                // System
                new() { Code = "SYSTEM_ADMIN", Name = "System Admin", Description = "Full system access", Group = "System" },
                
                // Menu Features
                new() { Code = "MENU_DASHBOARD", Name = "View Dashboard", Description = "Access Dashboard Menu", Group = "Menu" },
                new() { Code = "MENU_SETTINGS", Name = "View Settings", Description = "Access Settings Menu", Group = "Menu" },
                new() { Code = "MENU_USERS", Name = "View Users", Description = "Access Users Menu", Group = "Menu" },

                // Data Authorization
                new() { Code = "DATA_VIEW_ALL", Name = "View All Data", Description = "View data created by all users", Group = "Data" },
                new() { Code = "DATA_EDIT_ALL", Name = "Edit All Data", Description = "Edit data created by all users", Group = "Data" },
                new() { Code = "DATA_DELETE_ALL", Name = "Delete All Data", Description = "Delete data created by all users", Group = "Data" },
            };

            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();
        }
    }
}
