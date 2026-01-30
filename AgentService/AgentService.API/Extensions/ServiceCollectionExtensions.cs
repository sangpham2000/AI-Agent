using AgentService.API.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace AgentService.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebAPIServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddControllers();
        services.AddHealthChecks();
        services.AddSignalR();
        services.AddOpenApi(); // or AddSwaggerGen depending on preference, staying with current Program.cs
        
        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.WithOrigins("http://localhost:9999", "http://localhost:5173") // Added 5173 for local vite dev
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        // Authentication & Authorization
        services.AddAuthConfiguration(configuration, environment);

        return services;
    }

    private static IServiceCollection AddAuthConfiguration(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddHttpContextAccessor();

        // Check if we should bypass auth in development
        var bypassAuth = environment.IsDevelopment() && 
            configuration.GetValue<bool>("Development:BypassAuthentication", true);

        if (bypassAuth)
        {
            // Development mode - allow anonymous access
            services.AddAuthentication("DevAuth")
                .AddScheme<AuthenticationSchemeOptions, DevAuthHandler>("DevAuth", null);
            
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAssertion(_ => true) // Always allow in dev mode
                    .Build();
                options.AddPolicy("AdminOnly", policy => policy.RequireAssertion(_ => true));
                options.AddPolicy("UserOnly", policy => policy.RequireAssertion(_ => true));
            });
        }
        else
        {
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = configuration["Keycloak:Authority"];
                    options.Audience = configuration["Keycloak:Audience"];
                    options.RequireHttpsMetadata = false;
                    
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = configuration.GetValue<bool>("Keycloak:ValidateIssuer", true),
                        ValidateAudience = configuration.GetValue<bool>("Keycloak:ValidateAudience", true),
                        ValidateLifetime = true,
                        ValidIssuer = configuration["Keycloak:Authority"],
                        ValidAudience = configuration["Keycloak:Audience"],
                        RoleClaimType = "realm_access",
                        NameClaimType = "preferred_username"
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
                options.AddPolicy("UserOnly", policy => policy.RequireRole("user"));
            });
        }

        return services;
    }
}
