using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AgentService.Application.Interfaces.Repositories;
using AgentService.Application.Interfaces.Services;
using AgentService.Application.UseCases.Chat;
using AgentService.Infrastructure.Data;
using AgentService.Infrastructure.Repositories;
using AgentService.Infrastructure.Services;
using Telegram.Bot;

namespace AgentService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentException("DefaultConnection is required");
            
        services.AddDbContext<ApplicationReadDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddDbContext<ApplicationWriteDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Register DbContext as interface
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationWriteDbContext>());

        services.AddScoped<IUserQueryService, UserQueryService>();
        services.AddScoped<IUserWriteRepository, UserWriteRepository>();
        services.AddScoped<IRoleQueryService, RoleQueryService>();
        services.AddScoped<IRoleWriteRepository, RoleWriteRepository>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
            typeof(AgentService.Application.UseCases.Chat.GetConversationQueryHandler).Assembly,
            typeof(DependencyInjection).Assembly
        ));

        // Mapster
        AgentService.Application.Mappings.MappingConfig.Configure();
        services.AddMapster();

        // Services
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IQuotaService, QuotaService>();
        
        // Flowise Service
        services.AddHttpClient<IFlowiseService, FlowiseService>(client =>
        {
            var baseUrl = configuration["Flowise:BaseUrl"] ?? "http://localhost:3000";
            client.BaseAddress = new Uri(baseUrl);
            
            var apiKey = configuration["Flowise:ApiKey"];
            if (!string.IsNullOrEmpty(apiKey))
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            }
        });

        // Keycloak Admin Service
        services.AddHttpClient<IKeycloakAdminService, KeycloakAdminService>();

        // Telegram Bot
        var botToken = configuration["Telegram:BotToken"];
        if (!string.IsNullOrEmpty(botToken))
        {
            services.AddSingleton<ITelegramBotClient>(sp => 
            {
                var client = new TelegramBotClient(botToken, new HttpClient 
                { 
                    Timeout = TimeSpan.FromSeconds(180) 
                });
                return client;
            });
            services.AddScoped<ITelegramService, TelegramService>();
            
            // Register Hosted Service for Polling (Always on for local dev as requested)
            services.AddHostedService<BackgroundJobs.TelegramPollingService>();
        }
        else
        {
            // Register dummy/null implementations if token missing to prevent startup crash? 
            // Or just log warning. For now let's just not register to avoid crash, but app won't have bot.
        }
        
        // Telegram Bot Service (Legacy/Duplicate? Commenting out to ensure we use the new TelegramService)
        // services.AddHttpClient<ITelegramBotService, TelegramBotService>();

        return services;
    }
}
