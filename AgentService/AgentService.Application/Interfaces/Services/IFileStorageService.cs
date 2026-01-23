namespace AgentService.Application.Interfaces.Services;

public interface IFileStorageService
{
    /// <summary>
    /// Save stream to file and return the file path
    /// </summary>
    Task<FileUploadResult> SaveStreamAsync(Stream stream, string fileName, string? subFolder = null);
    
    /// <summary>
    /// Delete a file by path
    /// </summary>
    Task<bool> DeleteFileAsync(string filePath);
    
    /// <summary>
    /// Get file stream for download
    /// </summary>
    Task<Stream?> GetFileStreamAsync(string filePath);
    
    /// <summary>
    /// Check if file exists
    /// </summary>
    Task<bool> FileExistsAsync(string filePath);
}

public record FileUploadResult(
    bool Success,
    string? FilePath = null,
    string? FileName = null,
    long FileSize = 0,
    string? Error = null
);

