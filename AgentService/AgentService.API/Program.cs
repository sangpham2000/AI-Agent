using AgentService.API.Extensions;
using AgentService.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// Add infrastructure services (DbContext, MediatR, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Add Web API services (Controllers, Auth, CORS, SignalR, Swagger)
builder.Services.AddWebAPIServices(builder.Configuration, builder.Environment);

var app = builder.Build();

// Configure HTTP Pipeline (Middleware, Seeding, Endpoints)
app.UseSerilogRequestLogging(); // Add Serilog request logging
await app.ConfigurePipelineAsync();

app.Run();
