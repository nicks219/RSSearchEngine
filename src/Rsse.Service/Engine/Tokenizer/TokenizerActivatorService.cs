using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SearchEngine.Engine.Contracts;

namespace SearchEngine.Engine.Tokenizer;

/// <summary>
/// Сервис запуска по расписанию инициализации функционала токенизатора
/// </summary>
internal class TokenizerActivatorService(ITokenizerService tokenizer, ILogger<TokenizerActivatorService> logger) : BackgroundService
{
    // одни сутки:
    private const int BaseMs = 1000;
    private const int Min = 60;
    private const int Hour = 60;
    private const int Day = 24;
    private const int WaitMs = 1 * Day * Hour * Min * BaseMs;

    private int _count = 1;

    /// <summary>
    /// Инициализация токенайзера по расписанию
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("[TokenizerActivatorService] is active, prepare to runs for '{Count}' time", _count);

                _count++;

                tokenizer.Initialize();

                await Task.Delay(WaitMs, stoppingToken);
            }
        }
        finally
        {
            logger.LogInformation("[{Name}] graceful shutdown, cycles counter: '{Count}'", nameof(TokenizerActivatorService), _count.ToString());
        }
    }
}
