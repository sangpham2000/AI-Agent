using AgentService.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace AgentService.Infrastructure.Data.Seeders;

public static class RoleSeeder
{
    public static async Task SeedAsync(ApplicationWriteDbContext context)
    {
        if (!context.Roles.Any())
        {
            var adminRole = new Role { Name = "Admin", Description = "System Administrator", IsSystemRole = true };
            var managerRole = new Role { Name = "Manager", Description = "Business Manager", IsSystemRole = false };
            var userRole = new Role { Name = "User", Description = "Standard User", IsSystemRole = false };

            var roles = new List<Role> { adminRole, managerRole, userRole };

            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync(); // Save roles first to get IDs

            // Assign permissions
            var permissions = await context.Permissions.ToListAsync();
            
            // Admin gets all permissions
            foreach (var perm in permissions)
            {
                context.RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = perm.Id });
            }

            // Manager gets some permissions (e.g., all Menu + Data View/Edit)
            var managerPerms = permissions.Where(p => p.Group == "Menu" || p.Code == "DATA_VIEW_ALL" || p.Code == "DATA_EDIT_ALL");
            foreach (var perm in managerPerms)
            {
                context.RolePermissions.Add(new RolePermission { RoleId = managerRole.Id, PermissionId = perm.Id });
            }

            // User gets minimal permissions (e.g., Dashboard + View Users)
            var userPerms = permissions.Where(p => p.Code == "MENU_DASHBOARD" || p.Code == "MENU_USER_PROFILE"); 
            // Note: MENU_USER_PROFILE might not exist, checking PermissionSeeder.cs again... 
            // It has MENU_USERS. Let's stick to existing ones.

            var standardUserPerms = permissions.Where(p => p.Code == "MENU_DASHBOARD" || p.Code == "DATA_VIEW_ALL");
            foreach (var perm in standardUserPerms)
            {
                context.RolePermissions.Add(new RolePermission { RoleId = userRole.Id, PermissionId = perm.Id });
            }

            await context.SaveChangesAsync();
        }
    }
}
