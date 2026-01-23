# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY *.sln .
COPY AgentService.API/*.csproj ./AgentService.API/
COPY AgentService.Application/*.csproj ./AgentService.Application/
COPY AgentService.Infrastructure/*.csproj ./AgentService.Infrastructure/
COPY AgentService.Domain/*.csproj ./AgentService.Domain/
RUN dotnet restore

# Copy everything else and build
COPY . .

# Build
WORKDIR "/src/AgentService.API"
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "AgentService.API.dll"]

