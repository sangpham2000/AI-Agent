using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using AgentService.Application.Interfaces.Services;

namespace AgentService.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                // Fallback for keycloak 'sub' claim or 'preferred_username' logic if needed
                // For now assuming standard NameIdentifier
                return null;
            }
            return Guid.TryParse(userId, out var guid) ? guid : null;
        }
    }

    public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
}
