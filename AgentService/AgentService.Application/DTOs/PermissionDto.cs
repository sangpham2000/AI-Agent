namespace AgentService.Application.DTOs;

public record PermissionDto(Guid Id, string Code, string Name, string Description, string Group);
