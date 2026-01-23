using Microsoft.AspNetCore.Mvc;

namespace AgentService.API.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Health check endpoint for Docker and load balancers
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }

    /// <summary>
    /// Liveness probe for Kubernetes
    /// </summary>
    [HttpGet("live")]
    public IActionResult Live()
    {
        return Ok("alive");
    }

    /// <summary>
    /// Readiness probe for Kubernetes
    /// </summary>
    [HttpGet("ready")]
    public IActionResult Ready()
    {
        // Add checks for database, external services, etc.
        return Ok("ready");
    }
}
