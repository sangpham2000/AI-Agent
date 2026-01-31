using AgentService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

namespace AgentService.API.Extensions;

public static class WebApplicationExtensions
{
    public static async Task ConfigurePipelineAsync(this WebApplication app)
    {
        // Seed Database
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationWriteDbContext>();
            // Using MigrateAsync is safer in async context
            await context.Database.MigrateAsync(); 

            // Seed Data
            //await AgentService.Infrastructure.Data.Seeders.PermissionSeeder.SeedAsync(context);
            //await AgentService.Infrastructure.Data.Seeders.RoleSeeder.SeedAsync(context); 
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
            
            var bypassAuth = app.Configuration.GetValue<bool>("Development:BypassAuthentication", true);
            if (bypassAuth)
            {
                // We typically use logger instead of console in real apps, but console is fine for dev start msg
                var logger = app.Services.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Development mode: Authentication is BYPASSED!");
            }
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHub<AgentService.API.Hubs.ChatHub>("/chatHub"); // Ensure correct namespace
        app.MapHealthChecks("/api/health");
    }
}
