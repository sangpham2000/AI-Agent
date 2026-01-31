using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgentService.Application.DTOs.Documents;
using AgentService.Application.Interfaces.Services;
using AgentService.Application.UseCases.Documents;

namespace AgentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public DocumentsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Upload a document (PDF, DOCX)
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50MB limit
    public async Task<ActionResult<UploadDocumentResponse>> Upload(
        IFormFile file,
        [FromForm] string? title = null,
        [FromForm] string? category = null,
        [FromForm] string? description = null)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        var allowedExtensions = new[] { ".pdf", ".docx", ".doc", ".txt", ".md" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(extension))
            return BadRequest($"Invalid file type. Allowed: {string.Join(", ", allowedExtensions)}");

        // Create command with stream data
        await using var stream = file.OpenReadStream();
        var command = new UploadDocumentCommand(
            stream,
            file.FileName,
            file.ContentType,
            file.Length,
            title,
            category,
            description,
            _currentUserService.UserId
        );

        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// List all documents with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ListDocumentsResponse>> List(
        [FromQuery] string? category = null,
        [FromQuery] bool? isProcessed = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new ListDocumentsQuery(category, isProcessed, page, pageSize);
        var response = await _mediator.Send(query);
        return Ok(response);
    }

    /// <summary>
    /// Get document details by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DocumentDetailDto>> Get(Guid id)
    {
        var query = new GetDocumentQuery(id);
        var document = await _mediator.Send(query);

        if (document == null)
            return NotFound();

        return Ok(document);
    }

    /// <summary>
    /// Delete a document
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteDocumentCommand(id, _currentUserService.UserId);
        var result = await _mediator.Send(command);

        if (!result)
            return NotFound();

        return NoContent();
    }
}
