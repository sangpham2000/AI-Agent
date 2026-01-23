using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AgentService.Application.Interfaces.Services;

namespace AgentService.Infrastructure.Services;

public class FlowiseService : IFlowiseService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FlowiseService> _logger;
    private readonly string _chatflowId;
    private readonly string? _ingestionFlowId;
    private readonly JsonSerializerOptions _jsonOptions;

    public FlowiseService(HttpClient httpClient, IConfiguration configuration, ILogger<FlowiseService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _chatflowId = configuration["Flowise:ChatflowId"] ?? throw new ArgumentException("Flowise:ChatflowId is required");
        _ingestionFlowId = configuration["Flowise:IngestionFlowId"];
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task<FlowiseResponse> SendMessageAsync(string message, string? sessionId = null, Dictionary<string, object>? overrideConfig = null)
    {
        try
        {
            var request = new FlowiseRequest
            {
                Question = message,
                SessionId = sessionId,
                OverrideConfig = overrideConfig
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"/api/v1/prediction/{_chatflowId}",
                request,
                _jsonOptions
            );

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<FlowiseApiResponse>(_jsonOptions);

            return new FlowiseResponse(
                result?.Text ?? string.Empty,
                result?.SessionId,
                result?.SourceDocuments?.Select(sd => new SourceDocument(
                    sd.PageContent,
                    sd.Metadata?.GetValueOrDefault("source")?.ToString(),
                    sd.Metadata?.ContainsKey("page") == true ? int.Parse(sd.Metadata["page"].ToString()!) : null,
                    sd.Metadata
                )).ToList()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Flowise API");
            throw;
        }
    }

    public async Task<FlowiseResponse> SendMessageWithHistoryAsync(string message, List<ChatMessage> history, string? sessionId = null)
    {
        // Flowise handles history via sessionId, so we just pass the message
        // The history is maintained by Flowise internally when using persistent sessions
        return await SendMessageAsync(message, sessionId);
    }
    
    public async Task<bool> IngestDocumentAsync(Stream fileStream, string fileName)
    {
        if (string.IsNullOrEmpty(_ingestionFlowId))
        {
            _logger.LogWarning("Flowise:IngestionFlowId is not configured. Skipping ingestion.");
            return false;
        }

        try
        {
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }

            using var content = new MultipartFormDataContent();
            
            // Serialize overrideConfig as JSON
            var config = new Dictionary<string, object>
            {
                { "chunkSize", 1000 },
                { "chunkOverlap", 200 }
            };
            var jsonConfig = JsonSerializer.Serialize(config, _jsonOptions);
            
            byte[] fileBytes;
            using (var memoryStream = new MemoryStream())
            {
                await fileStream.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }
            
            _logger.LogInformation("File size in bytes: {Size}", fileBytes.Length);
            
            var fileContent = new ByteArrayContent(fileBytes);
            
            string contentType = "application/octet-stream";
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            switch (extension)
            {
                case ".txt": contentType = "text/plain"; break;
                case ".pdf": contentType = "application/pdf"; break;
                case ".docx": contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"; break;
                case ".doc": contentType = "application/msword"; break;
                case ".json": contentType = "application/json"; break;
            }
            
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            
            var uniqueFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{DateTime.UtcNow.Ticks}{extension}";
            
            content.Add(fileContent, "files", uniqueFileName);
            
            var response = await _httpClient.PostAsync(
                $"/api/v1/vector/upsert/{_ingestionFlowId}",
                content
            );

            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully ingested document {FileName}. Bytes sent: {Size}. Response: {Response}", 
                    fileName, fileBytes.Length, responseBody);
                return true;
            }
            else
            {
                _logger.LogError("Failed to ingest document to Flowise. Status: {Status}, Error: {Error}", 
                    response.StatusCode, responseBody);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document to Flowise");
            return false;
        }
    }

    private class FlowiseRequest
    {
        public string Question { get; set; } = string.Empty;
        public string? SessionId { get; set; }
        public Dictionary<string, object>? OverrideConfig { get; set; }
    }

    private class FlowiseApiResponse
    {
        public string Text { get; set; } = string.Empty;
        public string? SessionId { get; set; }
        public List<FlowiseSourceDocument>? SourceDocuments { get; set; }
    }

    private class FlowiseSourceDocument
    {
        public string PageContent { get; set; } = string.Empty;
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
