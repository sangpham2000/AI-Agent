using AgentService.Application.DTOs;
using AgentService.Application.UseCases.Chat; // For IApplicationDbContext
using AgentService.Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AgentService.Application.UseCases.Agents;

// --- Commands ---

public record CreateAgentCommand(CreateAgentDto Dto) : IRequest<Guid>;

public class CreateAgentCommandHandler : IRequestHandler<CreateAgentCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateAgentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateAgentCommand request, CancellationToken cancellationToken)
    {
        var agent = request.Dto.Adapt<Agent>();
        agent.Id = Guid.NewGuid();
        agent.CreatedAt = DateTime.UtcNow;

        // If this is default, unsettle others (optional logic)
        if (agent.IsDefault)
        {
            var defaults = await _context.Agents.Where(a => a.IsDefault).ToListAsync(cancellationToken);
            foreach (var d in defaults) d.IsDefault = false;
        }

        _context.Agents.Add(agent);
        await _context.SaveChangesAsync(cancellationToken);

        return agent.Id;
    }
}

public record UpdateAgentCommand(Guid Id, UpdateAgentDto Dto) : IRequest<bool>;

public class UpdateAgentCommandHandler : IRequestHandler<UpdateAgentCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateAgentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateAgentCommand request, CancellationToken cancellationToken)
    {
        var agent = await _context.Agents.FindAsync(new object[] { request.Id }, cancellationToken);
        if (agent == null) return false;

        if (request.Dto.Name != null) agent.Name = request.Dto.Name;
        if (request.Dto.Description != null) agent.Description = request.Dto.Description;
        if (request.Dto.FlowiseChatflowId != null) agent.FlowiseChatflowId = request.Dto.FlowiseChatflowId;
        if (request.Dto.SystemPrompt != null) agent.SystemPrompt = request.Dto.SystemPrompt;
        if (request.Dto.FlowiseConfig != null) agent.FlowiseConfig = request.Dto.FlowiseConfig;
        if (request.Dto.IsActive.HasValue) agent.IsActive = request.Dto.IsActive.Value;
        
        if (request.Dto.IsDefault.HasValue && request.Dto.IsDefault.Value)
        {
            agent.IsDefault = true;
            var defaults = await _context.Agents.Where(a => a.IsDefault && a.Id != agent.Id).ToListAsync(cancellationToken);
            foreach (var d in defaults) d.IsDefault = false;
        }
        else if (request.Dto.IsDefault.HasValue)
        {
             agent.IsDefault = request.Dto.IsDefault.Value;
        }

        agent.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record DeleteAgentCommand(Guid Id) : IRequest<bool>;
public class DeleteAgentCommandHandler : IRequestHandler<DeleteAgentCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteAgentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteAgentCommand request, CancellationToken cancellationToken)
    {
        var agent = await _context.Agents.FindAsync(new object[] { request.Id }, cancellationToken);
        if (agent == null) return false;
        
        _context.Agents.Remove(agent);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

// --- Queries ---

public record GetAgentsQuery() : IRequest<List<AgentDto>>;

public class GetAgentsQueryHandler : IRequestHandler<GetAgentsQuery, List<AgentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAgentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AgentDto>> Handle(GetAgentsQuery request, CancellationToken cancellationToken)
    {
        var agents = await _context.Agents.OrderByDescending(a => a.CreatedAt).ToListAsync(cancellationToken);
        return agents.Adapt<List<AgentDto>>();
    }
}

public record GetAgentByIdQuery(Guid Id) : IRequest<AgentDto?>;

public class GetAgentByIdQueryHandler : IRequestHandler<GetAgentByIdQuery, AgentDto?>
{
    private readonly IApplicationDbContext _context;

    public GetAgentByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AgentDto?> Handle(GetAgentByIdQuery request, CancellationToken cancellationToken)
    {
        var agent = await _context.Agents.FindAsync(new object[] { request.Id }, cancellationToken);
        return agent?.Adapt<AgentDto>();
    }
}
