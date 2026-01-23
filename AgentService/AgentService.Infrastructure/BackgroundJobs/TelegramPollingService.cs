using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using AgentService.Application.Interfaces.Services;

namespace AgentService.Infrastructure.BackgroundJobs;

public class TelegramPollingService : BackgroundService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TelegramPollingService> _logger;

    public TelegramPollingService(
        ITelegramBotClient botClient,
        IServiceProvider serviceProvider,
        ILogger<TelegramPollingService> logger)
    {
        _botClient = botClient;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
        };

        _logger.LogInformation("Starting Telegram Polling...");

        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken
        );

        // Keep the service alive
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Create a scope to resolve scoped services (like ITelegramService which might use scoped DbContext or logic)
        using var scope = _serviceProvider.CreateScope();
        var telegramService = scope.ServiceProvider.GetRequiredService<ITelegramService>();

        try
        {
            await telegramService.HandleUpdateAsync(update);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing update inside Polling Loop");
        }
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Telegram API Error");
        return Task.CompletedTask;
    }
}
