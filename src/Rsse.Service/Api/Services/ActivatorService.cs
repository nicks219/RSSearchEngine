using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SearchEngine.Service.Contracts;
using SearchEngine.Tooling;

namespace SearchEngine.Api.Services;

/// <summary>
/// Сервис запуска по расписанию инициализации функционала токенизатора.
/// </summary>
internal class ActivatorService(
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
        DatabaseInitializer.CreateAndSeed(factory, logger);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("[{Reporter}] is active, prepare to runs for '{Count}' time", nameof(ActivatorService), _count.ToString());

                _count++;

                await tokenizer.Initialize();

                logger.LogInformation("[{Reporter}] awaited for next start", nameof(ActivatorService));

                await Task.Delay(WaitMs, stoppingToken);
            }
        }
        finally
        {
            logger.LogInformation("[{Reporter}] graceful shutdown, cycles counter: '{Count}'", nameof(ActivatorService), _count.ToString());
        }
    }
}
