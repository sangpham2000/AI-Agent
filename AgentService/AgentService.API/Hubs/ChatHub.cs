using MediatR;
using Microsoft.AspNetCore.SignalR;
using AgentService.Application.Interfaces.Services;
using AgentService.Application.UseCases.Chat;

namespace AgentService.API.Hubs;

public class ChatHub : Hub
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public ChatHub(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    public async Task SendMessage(string user, string message, string model = "Gemini")
    {
        // 1. Broadcast user message immediately
        await Clients.All.SendAsync("ReceiveMessage", user, message);

        try 
        {
            // 2. Send message via command handler (saves to DB and calls Flowise)
            var command = new SendMessageCommand(
                ConversationId: null, // sẽ tạo mới nếu chưa có, hoặc cần caching sessionId -> conversationId
                Message: message,
                SessionId: Context.ConnectionId,
                Platform: "Web",
                UserId: _currentUserService.UserId,
                Model: model
            );
            
            var response = await _mediator.Send(command);

            // 3. Broadcast AI response
            await Clients.All.SendAsync("ReceiveMessage", "AI Agent", response.Response);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "System", $"Error: {ex.Message}");
        }
    }
}
