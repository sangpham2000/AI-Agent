using MediatR;
using Microsoft.AspNetCore.Mvc;
using AgentService.Application.Interfaces.Services;
using AgentService.Application.UseCases.Chat;

namespace AgentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TelegramController : ControllerBase
{
    private readonly ITelegramBotService _telegramBot;
    private readonly IMediator _mediator;
    private readonly ILogger<TelegramController> _logger;

    public TelegramController(
        ITelegramBotService telegramBot,
        IMediator mediator,
        ILogger<TelegramController> logger)
    {
        _telegramBot = telegramBot;
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Webhook endpoint for Telegram updates
    /// </summary>
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] TelegramUpdateDto update)
    {
        try
        {
            _logger.LogInformation("Received Telegram update: {UpdateId}", update.UpdateId);

            if (update.Message?.Text != null)
            {
                var chatId = update.Message.Chat.Id;
                var text = update.Message.Text;

                // Handle commands
                if (text.StartsWith("/"))
                {
                    var telegramUpdate = new TelegramUpdate(
                        update.UpdateId,
                        new TelegramMessage(
                            update.Message.MessageId,
                            update.Message.From != null 
                                ? new TelegramUser(update.Message.From.Id, update.Message.From.FirstName, update.Message.From.LastName, update.Message.From.Username)
                                : null,
                            new TelegramChat(update.Message.Chat.Id, update.Message.Chat.Type, update.Message.Chat.Title),
                            update.Message.Text,
                            DateTime.UtcNow
                        )
                    );
                    await _telegramBot.ProcessUpdateAsync(telegramUpdate);
                }
                else
                {
                    // Regular message - send to AI
                    var command = new SendMessageCommand(
                        null,
                        text,
                        chatId.ToString(),
                        "telegram",
                        null
                    );

                    var response = await _mediator.Send(command);

                    // Send response back to Telegram
                    await _telegramBot.SendMessageAsync(chatId, response.Response, update.Message.MessageId);
                }
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Telegram webhook");
            return Ok(); // Always return OK to Telegram to prevent retries
        }
    }

    /// <summary>
    /// Set webhook URL
    /// </summary>
    [HttpPost("set-webhook")]
    public async Task<IActionResult> SetWebhook([FromBody] SetWebhookRequest request)
    {
        var result = await _telegramBot.SetWebhookAsync(request.Url);
        return result ? Ok(new { message = "Webhook set successfully" }) : BadRequest("Failed to set webhook");
    }

    /// <summary>
    /// Get bot info
    /// </summary>
    [HttpGet("info")]
    public async Task<IActionResult> GetBotInfo()
    {
        var info = await _telegramBot.GetBotInfoAsync();
        return Ok(info);
    }
}

// DTOs for Telegram webhook
public record TelegramUpdateDto(
    long UpdateId,
    TelegramMessageDto? Message
);

public record TelegramMessageDto(
    int MessageId,
    TelegramUserDto? From,
    TelegramChatDto Chat,
    string? Text
);

public record TelegramUserDto(
    long Id,
    string? FirstName,
    string? LastName,
    string? Username
);

public record TelegramChatDto(
    long Id,
    string Type,
    string? Title
);

public record SetWebhookRequest(string Url);
