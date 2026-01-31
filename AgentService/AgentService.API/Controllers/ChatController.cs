using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgentService.Application.DTOs.Chat;
using AgentService.Application.Interfaces.Services;
using AgentService.Application.UseCases.Chat;

namespace AgentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public ChatController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Send a message and get AI response
    /// </summary>
    [HttpPost("send")]
    public async Task<ActionResult<SendMessageResponse>> SendMessage([FromBody] SendMessageRequest request)
    {
        var command = new SendMessageCommand(
            request.ConversationId,
            request.Message,
            request.SessionId,
            request.Platform,
            _currentUserService.UserId,
            request.Model
        );

        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// Get conversation by ID with all messages
    /// </summary>
    [HttpGet("conversations/{id:guid}")]
    public async Task<ActionResult<ConversationDetailDto>> GetConversation(Guid id)
    {
        var query = new GetConversationQuery(id, _currentUserService.UserId);
        var conversation = await _mediator.Send(query);

        if (conversation == null)
            return NotFound();

        return Ok(conversation);
    }

    /// <summary>
    /// List all conversations for current user or session
    /// </summary>
    [HttpGet("conversations")]
    public async Task<ActionResult<ListConversationsResponse>> ListConversations(
        [FromQuery] string? sessionId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new ListConversationsQuery(
            _currentUserService.UserId,
            sessionId,
            page,
            pageSize
        );

        var response = await _mediator.Send(query);
        return Ok(response);
    }

    /// <summary>
    /// Delete a conversation
    /// </summary>
    [HttpDelete("conversations/{id:guid}")]
    public async Task<IActionResult> DeleteConversation(Guid id)
    {
        var command = new DeleteConversationCommand(id, _currentUserService.UserId);
        var result = await _mediator.Send(command);

        if (!result)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Delete all conversations for current user
    /// </summary>
    [HttpDelete("conversations")]
    public async Task<IActionResult> DeleteAllConversations()
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Unauthorized();
        }

        var command = new DeleteAllConversationsCommand(_currentUserService.UserId.Value);
        var result = await _mediator.Send(command);

        if (!result)
            return NotFound("No active conversations found to delete.");

        return NoContent();
    }
}
