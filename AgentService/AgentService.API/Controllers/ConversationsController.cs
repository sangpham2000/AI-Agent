using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgentService.Application.DTOs.Chat;
using AgentService.Application.UseCases.Conversations;

namespace AgentService.API.Controllers;

/// <summary>
/// Admin controller for managing all conversations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class ConversationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConversationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// List all conversations (Admin only)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<AdminListConversationsResponse>> GetAll(
        [FromQuery] string? platform = null,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new AdminListConversationsQuery(
            platform,
            search,
            startDate,
            endDate,
            page,
            pageSize
        );

        var response = await _mediator.Send(query);
        return Ok(response);
    }

    /// <summary>
    /// Get conversation details with messages (Admin only)
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ConversationDetailDto>> GetById(Guid id)
    {
        var query = new AdminGetConversationQuery(id);
        var conversation = await _mediator.Send(query);

        if (conversation == null)
            return NotFound();

        return Ok(conversation);
    }

    /// <summary>
    /// Export conversation to file
    /// </summary>
    [HttpGet("{id:guid}/export")]
    public async Task<IActionResult> Export(Guid id, [FromQuery] string format = "json")
    {
        var query = new ExportConversationQuery(id, format);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        var contentType = format.ToLower() == "csv" 
            ? "text/csv" 
            : "application/json";

        return File(result.Data, contentType, result.FileName);
    }

    /// <summary>
    /// Delete a conversation (Admin only)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new AdminDeleteConversationCommand(id);
        var result = await _mediator.Send(command);

        if (!result)
            return NotFound();

        return NoContent();
    }
}
