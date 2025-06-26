using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SearchEngine.Services.Configuration;
using SearchEngine.Services.Contracts;
using SearchEngine.Tooling.MigrationAssistant;

namespace SearchEngine.Api.Services;

/// <summary>
/// Сервис запуска по расписанию инициализации функционала токенизатора.
/// </summary>
internal class ActivatorService(
    MigratorState migratorState,
    ITokenizerService tokenizer,
    IServiceScopeFactory factory,
    ILogger<ActivatorService> logger) : BackgroundService
{
    // одни сутки:
    private const int BaseMs = 1000;
    private const int Min = 60;
    private const int Hour = 60;
    private const int Day = 24;
    private const int WaitMs = 1 * Day * Hour * Min * BaseMs;

    private int _count = 1;

    /// <summary>
    /// Инициализация баз данных на старте сервиса и токенизатора по расписанию.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await DatabaseInitializer.CreateAndSeedAsync(factory, logger, stoppingToken);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var currentDateTime = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                logger.LogInformation("[{Reporter}] is active, prepare to runs for '{Count}' time | {Date}",
                    nameof(ActivatorService), _count.ToString(), currentDateTime);

                using (var scope = factory.CreateScope())
                {
                    var dataProvider = scope.ServiceProvider.GetRequiredService<DbDataProvider>();
                    await tokenizer.Initialize(dataProvider, stoppingToken);
                }

                logger.LogInformation("[{Reporter}] awaited for next start", nameof(ActivatorService));

                await Task.Delay(WaitMs, stoppingToken);

                _count++;
            }
        }
        finally
        {
            logger.LogInformation("[{Reporter}] graceful shutdown, cycles counter: '{Count}'", nameof(ActivatorService), _count.ToString());
        }
    }

    /// <summary>
    /// Попытаться дождаться завершения миграций при инициализации процесса остановки хоста.
    /// </summary>
    /// <param name="stoppingToken">Токен начала остановки хоста.</param>
    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        if (!migratorState.IsMigrating)
        {
            return;
        }

        logger.LogWarning("{Reporter} | graceful shutdown: waiting for migration to complete...",
            nameof(ActivatorService));

        try
        {

            var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(AppConstants.WaitMigratorTotalSeconds));
            while (migratorState.IsMigrating && !timeoutCts.IsCancellationRequested)
            {
                await Task.Delay(AppConstants.WaitMigratorNextCheckMs, timeoutCts.Token);
            }
        }
        finally
        {
            if (migratorState.IsMigrating)
            {
                logger.LogError("{Reporter} | shutdown timeout: migration not finished...", nameof(ActivatorService));
            }
            else
            {
                logger.LogInformation("{Reporter} | shutdown timeout: migration finished", nameof(ActivatorService));
            }
        }

        await base.StopAsync(stoppingToken);
    }
}
