using AgentService.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace AgentService.Infrastructure.Services;

public class TelegramService : ITelegramService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IFlowiseService _flowiseService;
    private readonly ILogger<TelegramService> _logger;

    public TelegramService(
        ITelegramBotClient botClient,
        IFlowiseService flowiseService,
        ILogger<TelegramService> logger)
    {
        _botClient = botClient;
        _flowiseService = flowiseService;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(Update update)
    {
        try
        {
            if (update.Message is not { } message)
                return;
            
            if (message.Text is not { } messageText)
                return;

            _logger.LogInformation("Received message from {ChatId}: {Text}", message.Chat.Id, messageText);
            
            var chatId = message.Chat.Id;
            
            if (messageText.StartsWith("/"))
            {
                var command = messageText.Split(' ')[0];

                switch (command)
                {
                    case "/start":
                        await SendMessageAsync(chatId, "Xin chào! Tôi là AI Agent của bạn.\nBạn cứ nhắn tin tự nhiên, tôi sẽ trả lời nhé!");
                        break;
                    case "/help":
                        await SendMessageAsync(chatId, "Danh sách lệnh:\n/start - Bắt đầu\n/help - Trợ giúp\n\nBạn có thể nhắn tin trực tiếp để hỏi tôi.");
                        break;
                    case "/ask":
                        await ProcessUserQuestion(chatId, messageText.Length > 5 ? messageText.Substring(5) : "");
                        break;
                    default:
                        await SendMessageAsync(chatId, "Lệnh không không hợp lệ. Hãy chat trực tiếp để dùng AI.");
                        break;
                }
            }
            else
            {
                // Natural chat mode
                await ProcessUserQuestion(chatId, messageText);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling Telegram update");
        }
    }

    private async Task ProcessUserQuestion(long chatId, string question)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            await SendMessageAsync(chatId, "Bạn muốn hỏi gì ạ?");
            return;
        }

        // Notify user we are thinking
        await _botClient.SendChatAction(chatId, ChatAction.Typing);

        try
        {
            // Call Flowise
            // SessionId can be ChatId to maintain context per user
            var response = await _flowiseService.SendMessageAsync(question, sessionId: chatId.ToString());
            
            if (response != null && !string.IsNullOrEmpty(response.Text))
            {
                await SendMessageAsync(chatId, response.Text);
            }
            else
            {
                await SendMessageAsync(chatId, "Xin lỗi, tôi không nhận được phản hồi từ AI System.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Flowise from Telegram");
            await SendMessageAsync(chatId, "Đã xảy ra lỗi khi xử lý yêu cầu của bạn.");
        }
    }

    public async Task SendMessageAsync(long chatId, string message)
    {
        await _botClient.SendMessage(chatId, message);
    }

    public async Task SetWebhookAsync(string webhookUrl)
    {
        await _botClient.SetWebhook(webhookUrl);
    }

    public async Task DeleteWebhookAsync()
    {
        await _botClient.DeleteWebhook();
    }
}
