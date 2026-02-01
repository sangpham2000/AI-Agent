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
    private readonly Dictionary<string, string> _models;
    private readonly string? _documentStoreId;
    private readonly string? _documentLoaderId;
    private readonly JsonSerializerOptions _jsonOptions;

    public FlowiseService(HttpClient httpClient, IConfiguration configuration, ILogger<FlowiseService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _documentStoreId = configuration["Flowise:DocumentStore:StoreId"];
        _documentLoaderId = configuration["Flowise:DocumentStore:LoaderId"];
        
        // Load models configuration
        _models = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        var modelsSection = configuration.GetSection("Flowise:Models");
        if (modelsSection.Exists())
        {
            foreach (var child in modelsSection.GetChildren())
            {
                if (!string.IsNullOrEmpty(child.Value))
                {
                    _models[child.Key] = child.Value;
                }
            }
        }
        else
        {
            // Backward compatibility for single ChatflowId
            var singleFlowId = configuration["Flowise:ChatflowId"];
            if (!string.IsNullOrEmpty(singleFlowId))
            {
                _models["Gemini"] = singleFlowId;
            }
        }
        
        if (!_models.Any())
        {
            _logger.LogWarning("No Flowise models configured!");
        }

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    private string GetChatflowId(string model)
    {
        if (_models.TryGetValue(model, out var chatflowId))
        {
            return chatflowId;
        }
        
        // Fallback to Gemini or first available if model not found
        if (_models.TryGetValue("Gemini", out var defaultId)) return defaultId;
        return _models.Values.FirstOrDefault() ?? throw new InvalidOperationException($"Model '{model}' not found and no default available.");
    }

    public async Task<FlowiseResponse> SendMessageAsync(string message, string? sessionId = null, Dictionary<string, object>? overrideConfig = null, string model = "Gemini")
    {
        try
        {
            var chatflowId = GetChatflowId(model);
            
            var request = new FlowiseRequest
            {
                Question = message,
                SessionId = sessionId,
                OverrideConfig = overrideConfig
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"/api/v1/prediction/{chatflowId}",
                request,
                _jsonOptions
            );

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<FlowiseApiResponse>(_jsonOptions);

            // Attempt to resolve token usage from various possible fields
            int? tokensUsed = result?.UsedTokens;
            
            // If explicit usedTokens is missing, check if we have a usage object
            if (!tokensUsed.HasValue && result?.Usage != null)
            {
                tokensUsed = result.Usage.TotalTokens;
            }

            var responseText = result?.Text ?? string.Empty;
            
            // Customize fallback response
            var fallbackPhrases = new[] 
            { 
                "Hmm, I'm not sure.", 
                "Hmm, tôi không chắc.", 
                "Tôi không chắc chắn.",
                "Tôi không tìm thấy thông tin.",
                "Xin lỗi, tôi không thể trả lời."
            };

            if (fallbackPhrases.Any(p => responseText.Trim().StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                responseText = "Xin lỗi, tôi chưa tìm thấy thông tin chính xác trong cơ sở dữ liệu. Bạn vui lòng cung cấp thêm chi tiết để tôi có thể hỗ trợ tốt hơn nhé.";
            }

            return new FlowiseResponse(
                responseText,
                result?.SessionId,
                result?.SourceDocuments?.Select(sd => new SourceDocument(
                    sd.PageContent,
                    sd.Metadata?.GetValueOrDefault("source")?.ToString(),
                    sd.Metadata?.ContainsKey("page") == true ? int.Parse(sd.Metadata["page"].ToString()!) : null,
                    sd.Metadata
                )).ToList(),
                tokensUsed
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Flowise API for model {Model}", model);
            throw;
        }
    }

    public async Task<FlowiseResponse> SendMessageWithHistoryAsync(string message, List<ChatMessage> history, string? sessionId = null, Dictionary<string, object>? overrideConfig = null, string model = "Gemini")
    {
        // Flowise handles history via sessionId, so we just pass the message
        // The history is maintained by Flowise internally when using persistent sessions
        return await SendMessageAsync(message, sessionId, overrideConfig, model);
    }
    
    public async Task<bool> IngestDocumentAsync(Stream fileStream, string fileName, Dictionary<string, object>? metadata = null)
    {
        if (string.IsNullOrEmpty(_documentStoreId) || string.IsNullOrEmpty(_documentLoaderId))
        {
            _logger.LogWarning("Flowise:DocumentStore is not fully configured. Skipping ingestion.");
            return false;
        }

        try
        {
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }

            using var content = new MultipartFormDataContent();
            
            byte[] fileBytes;
            using (var memoryStream = new MemoryStream())
            {
                await fileStream.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }
            
            _logger.LogInformation("Uploading {FileName} ({Size} bytes) to Document Store {StoreId}", fileName, fileBytes.Length, _documentStoreId);
            
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
            
            // Note: Flowise might expect the filename in the Content-Disposition
            content.Add(fileContent, "files", fileName);
            
            // Add required fields for Document Store API
            content.Add(new StringContent(_documentLoaderId), "docId");
            content.Add(new StringContent("true"), "replaceExisting");

            if (metadata != null && metadata.Any())
            {
                var metadataJson = JsonSerializer.Serialize(metadata, _jsonOptions);
                content.Add(new StringContent(metadataJson), "metadata");
            }

            // Execute Request
            var response = await _httpClient.PostAsync(
                $"/api/v1/document-store/upsert/{_documentStoreId}",
                content
            );

            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully ingested document {FileName} to Document Store. Response: {Response}", 
                    fileName, responseBody);
                return true;
            }
            else
            {
                _logger.LogError("Failed to ingest document to Document Store. Status: {Status}, Error: {Error}", 
                    response.StatusCode, responseBody);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document to Flowise Document Store");
            return false;
        }
    }

    public async Task<string> GenerateTitleAsync(string userMessage, string aiResponse)
    {
        try
        {
            // Limit context length to avoid huge prompts
            var context = $"User: {userMessage.Substring(0, Math.Min(userMessage.Length, 500))}\nAI: {aiResponse.Substring(0, Math.Min(aiResponse.Length, 500))}";
            var prompt = $"Summarize the following conversation into a short, concise title (max 5-7 words). Do not use quotes or prefixes like 'Title:'. just the title text itself.\n\n{context}";

            // Use a null sessionId to avoid polluting the main conversation history
            // Use the Default model (Gemini)
            var response = await SendMessageAsync(prompt, sessionId: null, model: "Gemini");
            
            var title = response.Text.Trim();
            
            // Cleanup quotes if present
            title = title.Trim('"').Trim('\'');
            
            // Fallback if AI refuses or fails
            if (string.IsNullOrWhiteSpace(title) || title.Length > 100)
            {
                return userMessage.Length > 50 ? userMessage[..47] + "..." : userMessage;
            }

            _logger.LogInformation("Generated AI Title: {Title}", title);
            
            return title;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate title via AI. Falling back to simple truncation.");
            return userMessage.Length > 50 ? userMessage[..47] + "..." : userMessage;
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
        
        // Common token usage fields from Flowise
        public int? UsedTokens { get; set; }
        public TokenUsage? Usage { get; set; }
    }

    private class TokenUsage 
    {
        public int? TotalTokens { get; set; }
        public int? PromptTokens { get; set; }
        public int? CompletionTokens { get; set; }
    }

    private class FlowiseSourceDocument
    {
        public string PageContent { get; set; } = string.Empty;
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
