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
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return null;

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? user.FindFirst("sub")?.Value 
                         ?? user.FindFirst("id")?.Value;

            if (userId == null) return null;

            return Guid.TryParse(userId, out var guid) ? guid : null;
        }
    }


    public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value 
                               ?? _httpContextAccessor.HttpContext?.User?.FindFirst("preferred_username")?.Value;

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value 
                            ?? _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value;

    public string? FirstName => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.GivenName)?.Value 
                                ?? _httpContextAccessor.HttpContext?.User?.FindFirst("given_name")?.Value;

    public string? LastName => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Surname)?.Value 
                               ?? _httpContextAccessor.HttpContext?.User?.FindFirst("family_name")?.Value;
}
