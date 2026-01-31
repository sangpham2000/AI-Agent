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
    private readonly IQuotaService _quotaService;
    private readonly Application.Interfaces.Repositories.IUserWriteRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public SendMessageCommandHandler(
        IApplicationDbContext context, 
        IFlowiseService flowiseService, 
        IQuotaService quotaService,
        Application.Interfaces.Repositories.IUserWriteRepository userRepository,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _flowiseService = flowiseService;
        _quotaService = quotaService;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<SendMessageResponse> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        // Sync user if authenticated to ensure FK constraints
        if (request.UserId.HasValue)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId.Value);
            if (user == null)
            {
                // Create user from claims
                user = new User
                {
                    Id = request.UserId.Value,
                    Username = _currentUserService.Username ?? "User",
                    Email = _currentUserService.Email ?? "",
                    FirstName = _currentUserService.FirstName ?? "User",
                    LastName = _currentUserService.LastName ?? "",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                await _userRepository.AddAsync(user);
            }
        }

        // Check quota if user is authenticated
        if (request.UserId.HasValue)
        {
            var hasQuota = await _quotaService.CheckQuotaAsync(request.UserId.Value);
            if (!hasQuota)
            {
                throw new InvalidOperationException("Monthly quota exceeded. Please contact support or upgrade your plan.");
            }
        }

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
            // Try to find an existing active conversation for this session/user
            // limit to conversations created in the last 24 hours to keep context fresh? 
            // For now, let's just find the latest one to keep the thread going.
            var existingConversation = await _context.Conversations
                .Where(c => c.IsActive)
                .Where(c => !string.IsNullOrEmpty(request.SessionId) && c.SessionId == request.SessionId)
                .OrderByDescending(c => c.UpdatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingConversation != null)
            {
                conversation = existingConversation;
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
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Conversations.Add(conversation);
            }
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
            conversation.Id.ToString(),
            request.Model
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

        // Generate AI Title if this is a new conversation (or title is generic/truncated)
        // We do this fire-and-forget style to avoid blocking the response, OR we await it if fast enough. 
        // For now, let's await it to ensure consistency, but wrap in try-catch to not fail the request.
        if (history.Count <= 2) // Just the first exchange
        {
            try 
            {
                var newTitle = await _flowiseService.GenerateTitleAsync(request.Message, flowiseResponse.Text);
                if (!string.IsNullOrWhiteSpace(newTitle))
                {
                    conversation.Title = newTitle;
                    // We need to save again to persist title. 
                    // Since we are monitoring the entity, just calling SaveChanges again is fine.
                }
            }
            catch (Exception ex)
            {
                // Log and ignore title generation failure
                Console.WriteLine($"Failed to generate title: {ex.Message}");
            }
        }
        
        await _context.SaveChangesAsync(cancellationToken);

        // Consume quota
        if (request.UserId.HasValue)
        {
             // Use actual tokens if available, otherwise estimate (e.g. 1 token per 4 chars ~ 1000 tokens default)
             var tokensUsed = flowiseResponse.TokensUsed ?? (request.Message.Length + flowiseResponse.Text.Length) / 4;
             if (tokensUsed < 1) tokensUsed = 1;
             
             await _quotaService.ConsumeQuotaAsync(request.UserId.Value, (int)tokensUsed);
        }

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
    DbSet<UserQuota> UserQuotas { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
