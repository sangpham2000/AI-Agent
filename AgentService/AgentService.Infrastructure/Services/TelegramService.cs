using MediatR;
using AgentService.Application.UseCases.Chat;
using AgentService.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.Extensions.DependencyInjection;
using AgentService.Domain.Entities;
using AgentService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

using DomainUser = AgentService.Domain.Entities.User;
using DomainTelegramUser = AgentService.Domain.Entities.TelegramUser;
using TelegramBotUser = Telegram.Bot.Types.User;

public class TelegramService : ITelegramService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IMediator _mediator;
    private readonly ILogger<TelegramService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public TelegramService(
        ITelegramBotClient botClient,
        IMediator mediator,
        ILogger<TelegramService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _botClient = botClient;
        _mediator = mediator;
        _logger = logger;
        _scopeFactory = scopeFactory;
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
            var telegramUser = await GetOrCreateTelegramUser(chatId, message.Chat.Username ?? "Unknown");

            // Handle commands or state flow
            if (messageText == "/start")
            {
                await SendRoleSelection(chatId);
                telegramUser.State = RegistrationState.SelectingRole;
                await UpdateTelegramUser(telegramUser);
                return;
            }

            // State Machine
            switch (telegramUser.State)
            {
                case RegistrationState.SelectingRole:
                    await HandleRoleSelection(chatId, messageText, telegramUser);
                    break;
                case RegistrationState.EnteringStudentId:
                    await HandleStudentIdEntry(chatId, messageText, telegramUser);
                    break;
                case RegistrationState.Completed:
                    var user = await GetUser(telegramUser.UserId);
                    var welcomeName = user?.FirstName ?? "bạn";
                    
                    switch (messageText.ToLower().Split(' ')[0])
                    {
                        case "/help":
                            await SendMessageAsync(chatId, "Danh sách lệnh:\n/start - Đăng ký/Bắt đầu\n/quota - Kiểm tra số dư Token\n/help - Trợ giúp\n\nBạn có thể nhắn tin trực tiếp để hỏi tôi.");
                            break;
                        case "/quota":
                            await HandleQuotaCheck(chatId, telegramUser);
                            break;
                        case "/ask":
                            await ProcessUserQuestion(chatId, messageText.Length > 5 ? messageText.Substring(5) : "");
                            break;
                        default:
                            await ProcessUserQuestion(chatId, messageText);
                            break;
                    }
                    break;
                default: 
                    // Should not happen if created correctly, reset to start
                     await SendRoleSelection(chatId);
                     telegramUser.State = RegistrationState.SelectingRole;
                     await UpdateTelegramUser(telegramUser);
                    break;
            }
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error handling Telegram update");
             await SendMessageAsync(update.Message?.Chat.Id ?? 0, "Đã xảy ra lỗi. Vui lòng thử lại sau.");
        }
    }

    private async Task HandleRoleSelection(long chatId, string text, DomainTelegramUser tUser)
    {
        if (text.Contains("Cán bộ", StringComparison.OrdinalIgnoreCase))
        {
            await CreateUserForTelegram(tUser, UserType.Staff, null);
            await SendMessageAsync(chatId, "Đăng ký thành công! Chào mừng Cán bộ. Bạn có thể bắt đầu hỏi tôi ngay.");
        }
        else if (text.Contains("Sinh viên", StringComparison.OrdinalIgnoreCase))
        {
            tUser.TempRole = UserType.Student;
            tUser.State = RegistrationState.EnteringStudentId;
            await UpdateTelegramUser(tUser);
            await SendMessageAsync(chatId, "Bạn vui lòng nhập Mã số sinh viên (MSSV):");
        }
        else
        {
            await SendMessageAsync(chatId, "Vui lòng chọn vai trò: 'Cán bộ' hoặc 'Sinh viên'.");
            await SendRoleSelection(chatId);
        }
    }

    private async Task HandleStudentIdEntry(long chatId, string text, DomainTelegramUser tUser)
    {
        if (text.Length < 3) // Basic validation
        {
            await SendMessageAsync(chatId, "Mã số sinh viên không hợp lệ. Vui lòng nhập lại:");
            return;
        }

        await CreateUserForTelegram(tUser, UserType.Student, text.Trim());
        await SendMessageAsync(chatId, $"Đăng ký thành công! Chào mừng Sinh viên {text}. Bạn có thể bắt đầu hỏi tôi ngay.");
    }

    private async Task HandleQuotaCheck(long chatId, DomainTelegramUser tUser)
    {
        if (tUser.State != RegistrationState.Completed || tUser.UserId == null)
        {
            await SendMessageAsync(chatId, "Bạn chưa đăng ký thành công. Vui lòng chat /start để đăng ký.");
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var quotaService = scope.ServiceProvider.GetRequiredService<IQuotaService>();
        
        var quota = await quotaService.GetOrCreateQuotaAsync(tUser.UserId.Value);
        
        var remaining = Math.Max(0, quota.MonthlyTokenLimit - quota.UsedTokens);
        var message = $"📊 *Thông tin Quota (Gemini)*\n\n" +
                      $"🔹 Giới hạn tháng: {quota.MonthlyTokenLimit:N0} tokens\n" +
                      $"🔸 Đã sử dụng: {quota.UsedTokens:N0} tokens\n" +
                      $"✅ Còn lại: {remaining:N0} tokens\n" +
                      $"📅 Ngày reset: {quota.LastResetDate.AddMonths(1):dd/MM/yyyy}";

        await SendMessageAsync(chatId, message);
    }

    private async Task SendRoleSelection(long chatId)
    {
        var keyboard = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "👨‍🏫 Cán bộ", "👨‍🎓 Sinh viên" }
        })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };
        
        await _botClient.SendMessage(chatId, "Xin chào! Vui lòng chọn vai trò của bạn:", replyMarkup: keyboard);
    }
    
    // Database Helpers using Service Scope
    private async Task<DomainTelegramUser> GetOrCreateTelegramUser(long chatId, string username)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationWriteDbContext>(); // Use ApplicationWriteDbContext
        
        var tUser = await context.TelegramUsers.FindAsync(chatId);
        if (tUser == null)
        {
            tUser = new DomainTelegramUser 
            { 
                ChatId = chatId, 
                Username = username, 
                State = RegistrationState.None 
            };
            context.TelegramUsers.Add(tUser);
            await context.SaveChangesAsync();
        }
        return tUser;
    }

    private async Task UpdateTelegramUser(DomainTelegramUser tUser)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationWriteDbContext>();
        context.TelegramUsers.Update(tUser);
        await context.SaveChangesAsync();
    }
    
    private async Task CreateUserForTelegram(DomainTelegramUser tUser, UserType type, string? studentId)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationWriteDbContext>();
        
        // Determine Username: Use StudentId if available, otherwise Telegram Username
        var systemUsername = (!string.IsNullOrEmpty(studentId) && type == UserType.Student) 
            ? studentId 
            : (string.IsNullOrEmpty(tUser.Username) || tUser.Username == "Unknown" 
                ? $"User_{tUser.ChatId}" 
                : tUser.Username);

        // Check if user already exists
        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Username == systemUsername);
        Guid userId;

        if (existingUser != null)
        {
            // Reuse existing user
            userId = existingUser.Id;
            
            // Optional: Update name if not set? 
            // For now, let's assume the existing user is the source of truth, 
            // but we might want to ensure StudentId is set if it was missing.
            if (string.IsNullOrEmpty(existingUser.StudentId) && !string.IsNullOrEmpty(studentId))
            {
                existingUser.StudentId = studentId;
                existingUser.UserType = type; // Update type if upgraded
                context.Users.Update(existingUser);
            }
        }
        else
        {
            // Create New User
            var newUser = new DomainUser
            {
                Id = Guid.NewGuid(),
                Username = systemUsername, 
                FirstName = type == UserType.Student ? "Sinh viên" : "Cán bộ",
                LastName = !string.IsNullOrEmpty(studentId) ? studentId : tUser.Username ?? "Unknown",
                UserType = type,
                StudentId = studentId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(newUser);
            userId = newUser.Id;
        }
        
        // Update Telegram User
        tUser.UserId = userId;
        tUser.State = RegistrationState.Completed;
        context.TelegramUsers.Update(tUser);
        
        await context.SaveChangesAsync();
    }
    
    private async Task<DomainUser?> GetUser(Guid? userId)
    {
        if (userId == null) return null;
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationWriteDbContext>(); // Read context is also fine, but Write is safe
        return await context.Users.FindAsync(userId);
    }

    private async Task ProcessUserQuestion(long chatId, string question)
    {
        // ... (Keep existing logic but pass UserId?)
        
        if (string.IsNullOrWhiteSpace(question))
        {
            await SendMessageAsync(chatId, "Bạn muốn hỏi gì ạ?");
            return;
        }

        await _botClient.SendChatAction(chatId, ChatAction.Typing);

        try
        {
             // Retrieve linked UserId to pass to Command
             using var scope = _scopeFactory.CreateScope();
             var context = scope.ServiceProvider.GetRequiredService<ApplicationWriteDbContext>();
             var tUser = await context.TelegramUsers.FindAsync(chatId);

            var command = new SendMessageCommand(
                ConversationId: null,
                Message: question,
                SessionId: chatId.ToString(),
                Platform: "Telegram",
                UserId: tUser?.UserId, // Pass the linked UserId!
                Model: "Gemini"
            );

            var response = await _mediator.Send(command);
            
            if (response != null && !string.IsNullOrEmpty(response.Response))
            {
                await _botClient.SendMessage(chatId, response.Response, replyMarkup: new ReplyKeyboardRemove()); // Remove keyboard if present
            }
            else
            {
                await SendMessageAsync(chatId, "Xin lỗi, tôi không nhận được phản hồi từ AI System.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AI request from Telegram");
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
