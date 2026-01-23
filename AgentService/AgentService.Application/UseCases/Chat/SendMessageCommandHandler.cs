using MediatR;
using Microsoft.EntityFrameworkCore;
using AgentService.Application.DTOs.Chat;
using AgentService.Application.Interfaces.Services;
using AgentService.Domain.Entities;

namespace AgentService.Application.UseCases.Chat;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, SendMessageResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IFlowiseService _flowiseService;

    public SendMessageCommandHandler(IApplicationDbContext context, IFlowiseService flowiseService)
    {
        _context = context;
        _flowiseService = flowiseService;
    }

    public async Task<SendMessageResponse> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        // Get or create conversation
        Conversation conversation;
        
        if (request.ConversationId.HasValue)
        {
            conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId.Value, cancellationToken)
                ?? throw new InvalidOperationException("Conversation not found");
        }
        else
        {
            conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Platform = request.Platform,
                SessionId = request.SessionId,
                Title = request.Message.Length > 50 ? request.Message[..47] + "..." : request.Message,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Conversations.Add(conversation);
        }

        // Save user message
        var userMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            Role = "user",
            Content = request.Message,
            CreatedAt = DateTime.UtcNow
        };
        _context.Messages.Add(userMessage);

        // Get conversation history for context
        var history = await _context.Messages
            .Where(m => m.ConversationId == conversation.Id)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatMessage(m.Role, m.Content))
            .ToListAsync(cancellationToken);

        // Call Flowise AI
        var flowiseResponse = await _flowiseService.SendMessageWithHistoryAsync(
            request.Message,
            history,
            conversation.Id.ToString()
        );

        // Save AI response
        var assistantMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            Role = "assistant",
            Content = flowiseResponse.Text,
            TokensUsed = flowiseResponse.TokensUsed,
            Metadata = flowiseResponse.SourceDocuments != null 
                ? System.Text.Json.JsonSerializer.Serialize(flowiseResponse.SourceDocuments) 
                : null,
            CreatedAt = DateTime.UtcNow
        };
        _context.Messages.Add(assistantMessage);

        // Update conversation
        conversation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new SendMessageResponse(
            conversation.Id,
            assistantMessage.Id,
            flowiseResponse.Text,
            assistantMessage.Metadata
        );
    }
}

// Interface for DbContext (to be implemented in Infrastructure)
public interface IApplicationDbContext
{
    DbSet<Conversation> Conversations { get; }
    DbSet<Message> Messages { get; }
    DbSet<Document> Documents { get; }
    DbSet<DocumentChunk> DocumentChunks { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
