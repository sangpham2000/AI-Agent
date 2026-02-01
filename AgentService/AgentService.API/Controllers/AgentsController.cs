using AgentService.Application.DTOs;
using AgentService.Application.UseCases.Agents;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require Auth
public class AgentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AgentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<AgentDto>>> GetAll()
    {
        var result = await _mediator.Send(new GetAgentsQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AgentDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetAgentByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")] // Only admins can manage agents? Or maybe Staff
    public async Task<ActionResult<Guid>> Create([FromBody] CreateAgentDto dto)
    {
        var result = await _mediator.Send(new CreateAgentCommand(dto));
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<bool>> Update(Guid id, [FromBody] UpdateAgentDto dto)
    {
        var result = await _mediator.Send(new UpdateAgentCommand(id, dto));
        if (!result) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<bool>> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteAgentCommand(id));
        if (!result) return NotFound();
        return Ok(result);
    }
}
