namespace AgentService.Application.DTOs;

public record RoleDto(Guid Id, string Name, string Description, List<PermissionDto> Permissions);
