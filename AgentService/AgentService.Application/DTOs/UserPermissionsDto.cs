namespace AgentService.Application.DTOs;

public record UserPermissionsDto(bool IsAdmin, bool IsSuperAdmin, IEnumerable<string> Roles);
