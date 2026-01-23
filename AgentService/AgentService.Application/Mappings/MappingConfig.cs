using Mapster;
using AgentService.Application.DTOs;
using AgentService.Domain.Entities;

namespace AgentService.Application.Mappings;

public static class MappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<User, UserDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .TwoWays();
            
        // Add more explicit mappings here if needed. 
        // Mapster works well with convention-based mapping (matching property names).
    }
}
