using AgentService.API.Extensions;
using AgentService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add infrastructure services (DbContext, MediatR, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Add Web API services (Controllers, Auth, CORS, SignalR, Swagger)
builder.Services.AddWebAPIServices(builder.Configuration, builder.Environment);

var app = builder.Build();

// Configure HTTP Pipeline (Middleware, Seeding, Endpoints)
await app.ConfigurePipelineAsync();

app.Run();
