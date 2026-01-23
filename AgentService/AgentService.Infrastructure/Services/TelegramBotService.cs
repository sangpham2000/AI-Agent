using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AgentService.Application.Interfaces.Services;

namespace AgentService.Infrastructure.Services;

public class TelegramBotService : ITelegramBotService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TelegramBotService> _logger;
    private readonly string _botToken;
    private readonly JsonSerializerOptions _jsonOptions;

    public TelegramBotService(HttpClient httpClient, IConfiguration configuration, ILogger<TelegramBotService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _botToken = configuration["Telegram:BotToken"] ?? throw new ArgumentException("Telegram:BotToken is required");
        
        _httpClient.BaseAddress = new Uri($"https://api.telegram.org/bot{_botToken}/");
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task ProcessUpdateAsync(TelegramUpdate update)
    {
        if (update.Message?.Text == null)
            return;

        var chatId = update.Message.Chat.Id;
        var text = update.Message.Text;

        _logger.LogInformation("Received message from chat {ChatId}: {Text}", chatId, text);

        // Handle commands
        if (text.StartsWith("/"))
        {
            await HandleCommandAsync(chatId, text, update.Message.MessageId);
            return;
        }

        // For regular messages, we'll process through the chat service
        // This will be handled by TelegramController which calls the chat use case
    }

    private async Task HandleCommandAsync(long chatId, string text, int messageId)
    {
        var command = text.Split(' ')[0].ToLower();

        switch (command)
        {
            case "/start":
                await SendMessageAsync(chatId, 
                    "👋 Xin chào! Tôi là Trợ lý Ảo Giáo dục.\n\n" +
                    "Tôi có thể giúp bạn tra cứu quy chế, học liệu và giải đáp thắc mắc.\n\n" +
                    "Hãy đặt câu hỏi để bắt đầu!", messageId);
                break;
            
            case "/help":
                await SendMessageAsync(chatId,
                    "📚 Hướng dẫn sử dụng:\n\n" +
                    "• Gửi câu hỏi trực tiếp để được hỗ trợ\n" +
                    "• /start - Bắt đầu lại cuộc trò chuyện\n" +
                    "• /help - Xem hướng dẫn\n" +
                    "• /clear - Xóa lịch sử trò chuyện", messageId);
                break;
            
            case "/clear":
                await SendMessageAsync(chatId, "🗑️ Đã xóa lịch sử trò chuyện.", messageId);
                break;
            
            default:
                await SendMessageAsync(chatId, "❓ Lệnh không được hỗ trợ. Gõ /help để xem hướng dẫn.", messageId);
                break;
        }
    }

    public async Task SendMessageAsync(long chatId, string text, int? replyToMessageId = null)
    {
        try
        {
            var request = new
            {
                chat_id = chatId,
                text = text,
                reply_to_message_id = replyToMessageId,
                parse_mode = "HTML"
            };

            var response = await _httpClient.PostAsJsonAsync("sendMessage", request, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Telegram message to chat {ChatId}", chatId);
            throw;
        }
    }

    public async Task<bool> SetWebhookAsync(string webhookUrl)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("setWebhook", new { url = webhookUrl }, _jsonOptions);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<TelegramApiResponse>(_jsonOptions);
            return result?.Ok ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting Telegram webhook");
            return false;
        }
    }

    public async Task<TelegramBotInfo> GetBotInfoAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<TelegramApiResponse<TelegramBotApiInfo>>("getMe", _jsonOptions);
            
            if (response?.Result == null)
                throw new InvalidOperationException("Failed to get bot info");

            return new TelegramBotInfo(
                response.Result.Id,
                response.Result.Username,
                response.Result.FirstName
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Telegram bot info");
            throw;
        }
    }

    private class TelegramApiResponse
    {
        public bool Ok { get; set; }
    }

    private class TelegramApiResponse<T> : TelegramApiResponse
    {
        public T? Result { get; set; }
    }

    private class TelegramBotApiInfo
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
    }
}
