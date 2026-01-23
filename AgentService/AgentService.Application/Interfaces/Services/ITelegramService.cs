using Telegram.Bot.Types;

namespace AgentService.Application.Interfaces.Services;

public interface ITelegramService
{
    Task HandleUpdateAsync(Update update);
    Task SendMessageAsync(long chatId, string message);
    Task SetWebhookAsync(string webhookUrl);
    Task DeleteWebhookAsync();
}
