namespace AgentService.Application.Interfaces.Services;

public interface ITelegramBotService
{
    /// <summary>
    /// Process incoming webhook update from Telegram
    /// </summary>
    Task ProcessUpdateAsync(TelegramUpdate update);
    
    /// <summary>
    /// Send a text message to a Telegram chat
    /// </summary>
    Task SendMessageAsync(long chatId, string text, int? replyToMessageId = null);
    
    /// <summary>
    /// Set webhook URL for Telegram Bot
    /// </summary>
    Task<bool> SetWebhookAsync(string webhookUrl);
    
    /// <summary>
    /// Get bot info
    /// </summary>
    Task<TelegramBotInfo> GetBotInfoAsync();
}

public record TelegramUpdate(
    long UpdateId,
    TelegramMessage? Message
);

public record TelegramMessage(
    int MessageId,
    TelegramUser? From,
    TelegramChat Chat,
    string? Text,
    DateTime Date
);

public record TelegramUser(
    long Id,
    string? FirstName,
    string? LastName,
    string? Username
);

public record TelegramChat(
    long Id,
    string Type,
    string? Title
);

public record TelegramBotInfo(
    long Id,
    string Username,
    string FirstName
);
