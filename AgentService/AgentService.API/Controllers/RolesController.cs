using MediatR;
using Microsoft.AspNetCore.Mvc;
using AgentService.Application.DTOs;
using AgentService.Application.UseCases.Roles.Commands;
using AgentService.Application.UseCases.Roles.Queries;

namespace AgentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllRolesQuery());
        return Ok(result);
    }

    [HttpGet("permissions")]
    public async Task<ActionResult<IEnumerable<PermissionDto>>> GetAllPermissions()
    {
        var result = await _mediator.Send(new GetAllPermissionsQuery());
        return Ok(result);
    }

    [HttpPut("{id}/permissions")]
    public async Task<IActionResult> UpdatePermissions(Guid id, [FromBody] List<Guid> permissionIds)
    {
        var command = new UpdateRolePermissionsCommand(id, permissionIds);
        var success = await _mediator.Send(command);
        
        if (!success) return NotFound();
        
        return NoContent();
    }
}
