using MediatR;
using AgentService.Application.DTOs.Documents;

namespace AgentService.Application.UseCases.Documents;

public record UploadDocumentCommand(
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSize,
    string? Title,
    string? Category,
    string? Description,
    Guid? UserId
) : IRequest<UploadDocumentResponse>;

public record ListDocumentsQuery(
    string? Category,
    bool? IsProcessed,
    int Page = 1,
    int PageSize = 20
) : IRequest<ListDocumentsResponse>;

public record GetDocumentQuery(Guid DocumentId) : IRequest<DocumentDetailDto?>;
public record GetDocumentContentQuery(Guid DocumentId) : IRequest<string?>;

public record DeleteDocumentCommand(Guid DocumentId, Guid? UserId) : IRequest<bool>;

