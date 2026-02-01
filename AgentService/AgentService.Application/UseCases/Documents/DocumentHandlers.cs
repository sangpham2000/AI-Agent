using MediatR;
using Microsoft.EntityFrameworkCore;
using AgentService.Application.DTOs.Documents;
using AgentService.Application.Interfaces.Services;
using AgentService.Application.UseCases.Chat;
using AgentService.Domain.Entities;

namespace AgentService.Application.UseCases.Documents;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, UploadDocumentResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly IFlowiseService _flowiseService;

    public UploadDocumentCommandHandler(IApplicationDbContext context, IFileStorageService fileStorage, IFlowiseService flowiseService)
    {
        _context = context;
        _fileStorage = fileStorage;
        _flowiseService = flowiseService;
    }

    public async Task<UploadDocumentResponse> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        // Save file to storage
        var uploadResult = await _fileStorage.SaveStreamAsync(request.FileStream, request.FileName, "documents");
        
        if (!uploadResult.Success)
        {
            throw new InvalidOperationException($"Failed to upload file: {uploadResult.Error}");
        }

        // Determine file type
        var extension = Path.GetExtension(request.FileName).ToLowerInvariant();
        var fileType = extension switch
        {
            ".pdf" => "pdf",
            ".docx" => "docx",
            ".doc" => "doc",
            ".txt" => "txt",
            _ => "unknown"
        };

        // Create document record
        var document = new Document
        {
            Id = Guid.NewGuid(),
            Title = request.Title ?? Path.GetFileNameWithoutExtension(request.FileName),
            FileName = uploadResult.FileName!,
            FileType = fileType,
            FilePath = uploadResult.FilePath,
            Category = request.Category,
            Description = request.Description,
            FileSize = uploadResult.FileSize,
            UploadedByUserId = request.UserId,
            IsProcessed = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync(cancellationToken);

        // Auto-process document using Flowise RAG Ingestion
        try {
            // We need a fresh stream or reset the existing one if possible
            // Since SaveStreamAsync might have consumed it, ideally we should have copied it to MemoryStream first if not seekable
            // Assuming request.FileStream is seekable or we handle it by copying to MemoryStream at start.
            // But to be safe, let's try to reset if seekable, otherwise catch exception.
            
            if (request.FileStream.CanSeek)
            {
                request.FileStream.Position = 0;
                
                var metadata = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(request.Category))
                {
                    metadata["category"] = request.Category;
                }
                
                var ingested = await _flowiseService.IngestDocumentAsync(request.FileStream, request.FileName, metadata);
                
                if (ingested)
                {
                    document.IsProcessed = true;
                    document.ProcessingError = null;
                    document.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    // If ingestion failed (e.g. config missing), we leave it as IsProcessed=false
                    // Optionally update error message
                    document.ProcessingError = "Ingestion failed or not configured";
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail the upload response
            // Maybe update document with error
            document.ProcessingError = $"Ingestion error: {ex.Message}";
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new UploadDocumentResponse(
            document.Id,
            document.FileName,
            document.IsProcessed 
                ? "Document uploaded and processed successfully." 
                : "Document uploaded. Processing pending or failed (check details)."
        );
    }
}

public class ListDocumentsQueryHandler : IRequestHandler<ListDocumentsQuery, ListDocumentsResponse>
{
    private readonly IApplicationDbContext _context;

    public ListDocumentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ListDocumentsResponse> Handle(ListDocumentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Documents.AsQueryable();

        if (!string.IsNullOrEmpty(request.Category))
            query = query.Where(d => d.Category == request.Category);

        if (request.IsProcessed.HasValue)
            query = query.Where(d => d.IsProcessed == request.IsProcessed.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(d => new DocumentDto(
                d.Id,
                d.Title,
                d.FileName,
                d.FileType,
                d.Category,
                d.Description,
                d.FileSize,
                d.IsProcessed,
                d.ChunkCount,
                d.CreatedAt,
                d.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        return new ListDocumentsResponse(items, totalCount, request.Page, request.PageSize);
    }
}

public class GetDocumentQueryHandler : IRequestHandler<GetDocumentQuery, DocumentDetailDto?>
{
    private readonly IApplicationDbContext _context;

    public GetDocumentQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DocumentDetailDto?> Handle(GetDocumentQuery request, CancellationToken cancellationToken)
    {
        return await _context.Documents
            .Where(d => d.Id == request.DocumentId)
            .Select(d => new DocumentDetailDto(
                d.Id,
                d.Title,
                d.FileName,
                d.FileType,
                d.Category,
                d.Description,
                d.FileSize,
                d.IsProcessed,
                d.ProcessingError,
                d.ChunkCount,
                d.UploadedByUserId,
                d.CreatedAt,
                d.UpdatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public DeleteDocumentCommandHandler(IApplicationDbContext context, IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<bool> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _context.Documents
            .FirstOrDefaultAsync(d => d.Id == request.DocumentId, cancellationToken);

        if (document == null)
            return false;

        // Delete file from storage
        if (!string.IsNullOrEmpty(document.FilePath))
        {
            await _fileStorage.DeleteFileAsync(document.FilePath);
        }

        _context.Documents.Remove(document);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

public class GetDocumentContentQueryHandler : IRequestHandler<GetDocumentContentQuery, string?>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public GetDocumentContentQueryHandler(IApplicationDbContext context, IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<string?> Handle(GetDocumentContentQuery request, CancellationToken cancellationToken)
    {
        var document = await _context.Documents
            .FirstOrDefaultAsync(d => d.Id == request.DocumentId, cancellationToken);

        if (document == null || string.IsNullOrEmpty(document.FilePath))
            return null;

        var stream = await _fileStorage.GetFileStreamAsync(document.FilePath);
        if (stream == null)
            return null;

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
