namespace AgentService.Application.Interfaces.Services;

public interface IFlowiseService
{
    /// <summary>
    /// Send a message to Flowise and get AI response
    /// </summary>
    Task<FlowiseResponse> SendMessageAsync(string message, string? sessionId = null, Dictionary<string, object>? overrideConfig = null, string model = "Gemini");
    
    /// <summary>
    /// Send a message with conversation history for context
    /// </summary>
    Task<FlowiseResponse> SendMessageWithHistoryAsync(string message, List<ChatMessage> history, string? sessionId = null, Dictionary<string, object>? overrideConfig = null, string model = "Gemini");

    /// <summary>
    /// Upload and ingest a document into the vector store via Flowise
    /// </summary>
    Task<bool> IngestDocumentAsync(Stream fileStream, string fileName, Dictionary<string, object>? metadata = null);

    /// <summary>
    /// Generate a short title for the conversation based on user message and AI response
    /// </summary>
    Task<string> GenerateTitleAsync(string userMessage, string aiResponse);
}

public record FlowiseResponse(
    string Text,
    string? SessionId = null,
    List<SourceDocument>? SourceDocuments = null,
    int? TokensUsed = null
);

public record ChatMessage(
    string Role,
    string Content
);

public record SourceDocument(
    string PageContent,
    string? Source = null,
    int? PageNumber = null,
    Dictionary<string, object>? Metadata = null
);
