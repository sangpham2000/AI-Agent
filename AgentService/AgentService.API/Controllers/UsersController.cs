using MediatR;
using Microsoft.AspNetCore.Mvc;
using AgentService.Application.DTOs;
using AgentService.Application.UseCases.Users.Commands;
using AgentService.Application.UseCases.Users.Queries;

namespace AgentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllUsersQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserDto createUserDto)
    {
        var command = new CreateUserCommand(
            createUserDto.Username, 
            createUserDto.Email, 
            createUserDto.FirstName, 
            createUserDto.LastName, 
            createUserDto.PhoneNumber, 
            createUserDto.DateOfBirth, 
            createUserDto.AvatarUrl);

        var result = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateUserDto updateUserDto)
    {
        var command = new UpdateUserCommand(
            id,
            updateUserDto.Username,
            updateUserDto.Email,
            updateUserDto.FirstName,
            updateUserDto.LastName,
            updateUserDto.PhoneNumber,
            updateUserDto.DateOfBirth,
            updateUserDto.AvatarUrl,
            updateUserDto.IsActive);

        var success = await _mediator.Send(command);
        if (!success) return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteUserCommand(id));
        return NoContent();
    }

    [HttpPost("{id}/roles")]
    public async Task<IActionResult> AssignRole(Guid id, [FromBody] string roleName)
    {
        var success = await _mediator.Send(new AssignRoleCommand(id, roleName));
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpGet("{id}/permissions")]
    public async Task<ActionResult<UserPermissionsDto>> GetPermissions(Guid id)
    {
        var result = await _mediator.Send(new GetMyPermissionsQuery(id));
        return Ok(result);
    }
}
