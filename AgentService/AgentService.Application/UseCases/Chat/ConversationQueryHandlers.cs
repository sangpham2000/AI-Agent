using MediatR;
using Microsoft.EntityFrameworkCore;
using AgentService.Application.DTOs.Chat;

namespace AgentService.Application.UseCases.Chat;

public class GetConversationQueryHandler : IRequestHandler<GetConversationQuery, ConversationDetailDto?>
{
    private readonly IApplicationDbContext _context;

    public GetConversationQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ConversationDetailDto?> Handle(GetConversationQuery request, CancellationToken cancellationToken)
    {
        var conversation = await _context.Conversations
            .Where(c => c.Id == request.ConversationId)
            .Where(c => !request.UserId.HasValue || c.UserId == request.UserId)
            .Select(c => new ConversationDetailDto(
                c.Id,
                c.UserId.HasValue ? c.UserId.Value.ToString() : null,
                c.SessionId,
                c.Platform,
                c.Messages.OrderBy(m => m.CreatedAt).Select(m => new MessageDto(
                    m.Id,
                    m.Role,
                    m.Content,
                    m.CreatedAt,
                    m.Metadata
                )).ToList(),
                c.CreatedAt,
                c.UpdatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return conversation;
    }
}

public class ListConversationsQueryHandler : IRequestHandler<ListConversationsQuery, ListConversationsResponse>
{
    private readonly IApplicationDbContext _context;

    public ListConversationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ListConversationsResponse> Handle(ListConversationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Conversations.AsQueryable();

        if (request.UserId.HasValue)
            query = query.Where(c => c.UserId == request.UserId);
        else if (!string.IsNullOrEmpty(request.SessionId))
            query = query.Where(c => c.SessionId == request.SessionId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.UpdatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new ConversationDto(
                c.Id,
                c.Title,
                c.Platform,
                c.CreatedAt,
                c.UpdatedAt,
                c.Messages.Count
            ))
            .ToListAsync(cancellationToken);

        return new ListConversationsResponse(items, totalCount, request.Page, request.PageSize);
    }
}

public class DeleteConversationCommandHandler : IRequestHandler<DeleteConversationCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteConversationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _context.Conversations
            .Where(c => c.Id == request.ConversationId)
            .Where(c => !request.UserId.HasValue || c.UserId == request.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (conversation == null)
            return false;

        _context.Conversations.Remove(conversation);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
