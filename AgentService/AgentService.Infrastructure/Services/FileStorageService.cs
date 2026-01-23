using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using AgentService.Application.Interfaces.Services;

namespace AgentService.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(IWebHostEnvironment environment, ILogger<FileStorageService> logger)
    {
        _basePath = Path.Combine(environment.ContentRootPath, "Uploads");
        _logger = logger;
        
        // Ensure base directory exists
        Directory.CreateDirectory(_basePath);
    }

    public async Task<FileUploadResult> SaveStreamAsync(Stream stream, string fileName, string? subFolder = null)
    {
        try
        {
            var targetFolder = string.IsNullOrEmpty(subFolder) 
                ? _basePath 
                : Path.Combine(_basePath, subFolder);
            
            Directory.CreateDirectory(targetFolder);

            // Generate unique filename
            var uniqueFileName = $"{Guid.NewGuid()}_{SanitizeFileName(fileName)}";
            var filePath = Path.Combine(targetFolder, uniqueFileName);

            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await stream.CopyToAsync(fileStream);
            var fileSize = fileStream.Length;

            _logger.LogInformation("File saved successfully: {FilePath}", filePath);

            return new FileUploadResult(
                Success: true,
                FilePath: filePath,
                FileName: uniqueFileName,
                FileSize: fileSize
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file: {FileName}", fileName);
            return new FileUploadResult(
                Success: false,
                Error: ex.Message
            );
        }
    }

    public Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted: {FilePath}", filePath);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            return Task.FromResult(false);
        }
    }

    public Task<Stream?> GetFileStreamAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return Task.FromResult<Stream?>(null);

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return Task.FromResult<Stream?>(stream);
    }

    public Task<bool> FileExistsAsync(string filePath)
    {
        return Task.FromResult(File.Exists(filePath));
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return sanitized;
    }
}

