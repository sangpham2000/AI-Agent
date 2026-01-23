using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using AgentService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// CORS for Web Plugin
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Authentication with Keycloak
builder.Services.AddHttpContextAccessor(); // Required for CurrentUserService

// Check if we should bypass auth in development
var bypassAuth = builder.Environment.IsDevelopment() && 
    builder.Configuration.GetValue<bool>("Development:BypassAuthentication", true);

if (bypassAuth)
{
    // Development mode - allow anonymous access
    builder.Services.AddAuthentication("DevAuth")
        .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, DevAuthHandler>("DevAuth", null);
    
    builder.Services.AddAuthorization(options =>
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
    builder.Services.AddAuthentication("Bearer")
        .AddJwtBearer("Bearer", options =>
        {
            options.Authority = builder.Configuration["Keycloak:Authority"];
            options.Audience = builder.Configuration["Keycloak:Audience"];
            options.RequireHttpsMetadata = false;
            
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = builder.Configuration.GetValue<bool>("Keycloak:ValidateIssuer", true),
                ValidateAudience = builder.Configuration.GetValue<bool>("Keycloak:ValidateAudience", true),
                ValidateLifetime = true,
                ValidIssuer = builder.Configuration["Keycloak:Authority"],
                ValidAudience = builder.Configuration["Keycloak:Audience"],
                RoleClaimType = "realm_access",
                NameClaimType = "preferred_username"
            };
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
        options.AddPolicy("UserOnly", policy => policy.RequireRole("user"));
    });
}

var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AgentService.Infrastructure.Data.ApplicationWriteDbContext>();
    context.Database.Migrate(); // Ensure DB is created/migrated
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    
    if (bypassAuth)
    {
        Console.WriteLine("⚠️  Development mode: Authentication is BYPASSED!");
    }
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/api/health");

app.Run();

// Development authentication handler - allows all requests
public class DevAuthHandler : Microsoft.AspNetCore.Authentication.AuthenticationHandler<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions>
{
    public DevAuthHandler(
        Microsoft.Extensions.Options.IOptionsMonitor<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions> options,
        Microsoft.Extensions.Logging.ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder) 
        : base(options, logger, encoder) { }

    protected override Task<Microsoft.AspNetCore.Authentication.AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "dev-user"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "Development User"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "dev@localhost"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "admin"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "user"),
        };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, Scheme.Name);
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        var ticket = new Microsoft.AspNetCore.Authentication.AuthenticationTicket(principal, Scheme.Name);
        
        return Task.FromResult(Microsoft.AspNetCore.Authentication.AuthenticateResult.Success(ticket));
    }
}
