namespace AgentService.Application.DTOs.Documents;

public record DocumentDto(
    Guid Id,
    string Title,
    string FileName,
    string FileType,
    string? Category,
    string? Description,
    long FileSize,
    bool IsProcessed,
    int ChunkCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record DocumentDetailDto(
    Guid Id,
    string Title,
    string FileName,
    string FileType,
    string? Category,
    string? Description,
    long FileSize,
    bool IsProcessed,
    string? ProcessingError,
    int ChunkCount,
    Guid? UploadedByUserId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record ListDocumentsRequest(
    string? Category = null,
    bool? IsProcessed = null,
    int Page = 1,
    int PageSize = 20
);

public record ListDocumentsResponse(
    List<DocumentDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public record UploadDocumentResponse(
    Guid Id,
    string FileName,
    string Message
);

