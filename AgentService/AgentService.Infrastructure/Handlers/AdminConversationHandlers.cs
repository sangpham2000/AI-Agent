using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using AgentService.Application.DTOs.Chat;
using AgentService.Infrastructure.Data;

namespace AgentService.Application.UseCases.Conversations;

/// <summary>
/// Handler for listing all conversations (Admin)
/// </summary>
public class AdminListConversationsHandler : IRequestHandler<AdminListConversationsQuery, AdminListConversationsResponse>
{
    private readonly ApplicationReadDbContext _context;

    public AdminListConversationsHandler(ApplicationReadDbContext context)
    {
        _context = context;
    }

    public async Task<AdminListConversationsResponse> Handle(AdminListConversationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Conversations.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.Platform))
        {
            query = query.Where(c => c.Platform == request.Platform);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= request.EndDate.Value);
        }

        if (!string.IsNullOrEmpty(request.Search))
        {
            query = query.Where(c => 
                (c.SessionId != null && c.SessionId.Contains(request.Search)) ||
                (c.UserId.HasValue && c.UserId.Value.ToString().Contains(request.Search)));
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Get paginated results
        var conversations = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new ConversationSummaryDto(
                c.Id,
                c.UserId.HasValue ? c.UserId.Value.ToString() : null,
                null, // UserName - would need to join with Users table
                null, // UserEmail - would need to join with Users table
                c.Platform ?? "Unknown",
                c.Messages.Count,
                c.CreatedAt,
                c.UpdatedAt,
                c.Messages.Any(m => m.CreatedAt > DateTime.UtcNow.AddMinutes(-5)) ? "active" : "completed"
            ))
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new AdminListConversationsResponse(
            conversations,
            totalCount,
            request.Page,
            request.PageSize,
            totalPages
        );
    }
}

/// <summary>
/// Handler for getting a conversation by ID (Admin)
/// </summary>
public class AdminGetConversationHandler : IRequestHandler<AdminGetConversationQuery, ConversationDetailDto?>
{
    private readonly ApplicationReadDbContext _context;

    public AdminGetConversationHandler(ApplicationReadDbContext context)
    {
        _context = context;
    }

    public async Task<ConversationDetailDto?> Handle(AdminGetConversationQuery request, CancellationToken cancellationToken)
    {
        var conversation = await _context.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken);

        if (conversation == null)
            return null;

        var messages = conversation.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageDto(
                m.Id,
                m.Role,
                m.Content,
                m.CreatedAt
            ))
            .ToList();

        return new ConversationDetailDto(
            conversation.Id,
            conversation.UserId?.ToString(),
            conversation.SessionId,
            conversation.Platform ?? "Unknown",
            messages,
            conversation.CreatedAt,
            conversation.UpdatedAt
        );
    }
}

/// <summary>
/// Handler for exporting a conversation
/// </summary>
public class ExportConversationHandler : IRequestHandler<ExportConversationQuery, ExportConversationResult?>
{
    private readonly ApplicationReadDbContext _context;

    public ExportConversationHandler(ApplicationReadDbContext context)
    {
        _context = context;
    }

    public async Task<ExportConversationResult?> Handle(ExportConversationQuery request, CancellationToken cancellationToken)
    {
        var conversation = await _context.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken);

        if (conversation == null)
            return null;

        byte[] data;
        string fileName;

        if (request.Format.ToLower() == "csv")
        {
            // Export as CSV
            var sb = new StringBuilder();
            sb.AppendLine("Timestamp,Role,Content");
            
            foreach (var message in conversation.Messages.OrderBy(m => m.CreatedAt))
            {
                var content = message.Content.Replace("\"", "\"\""); // Escape quotes
                sb.AppendLine($"\"{message.CreatedAt:yyyy-MM-dd HH:mm:ss}\",\"{message.Role}\",\"{content}\"");
            }

            data = Encoding.UTF8.GetBytes(sb.ToString());
            fileName = $"conversation_{conversation.Id}.csv";
        }
        else
        {
            // Export as JSON
            var exportData = new
            {
                Id = conversation.Id,
                Platform = conversation.Platform,
                UserId = conversation.UserId,
                SessionId = conversation.SessionId,
                CreatedAt = conversation.CreatedAt,
                UpdatedAt = conversation.UpdatedAt,
                Messages = conversation.Messages.OrderBy(m => m.CreatedAt).Select(m => new
                {
                    Role = m.Role,
                    Content = m.Content,
                    Timestamp = m.CreatedAt
                })
            };

            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            data = JsonSerializer.SerializeToUtf8Bytes(exportData, jsonOptions);
            fileName = $"conversation_{conversation.Id}.json";
        }

        return new ExportConversationResult(data, fileName);
    }
}

/// <summary>
/// Handler for deleting a conversation (Admin)
/// </summary>
public class AdminDeleteConversationHandler : IRequestHandler<AdminDeleteConversationCommand, bool>
{
    private readonly ApplicationWriteDbContext _context;

    public AdminDeleteConversationHandler(ApplicationWriteDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(AdminDeleteConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _context.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken);

        if (conversation == null)
            return false;

        // Remove messages first
        _context.Messages.RemoveRange(conversation.Messages);
        
        // Remove conversation
        _context.Conversations.Remove(conversation);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return true;
    }
}
