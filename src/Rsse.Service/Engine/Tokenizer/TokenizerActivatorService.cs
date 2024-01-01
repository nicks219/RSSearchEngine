using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SearchEngine.Engine.Contracts;

namespace SearchEngine.Engine.Tokenizer;

/// <summary>
/// Сервис запуска по расписанию инициализации функционала токенизатора
/// </summary>
internal class TokenizerActivatorService : BackgroundService
{
    // одни сутки:
    private const int BaseMs = 1000;
    private const int Min = 60;
    private const int Hour = 60;
    private const int Day = 24;
    private const int WaitMs = 1 * Day * Hour * Min * BaseMs;

    private readonly ITokenizerService _tokenizer;
    private readonly ILogger<TokenizerActivatorService> _logger;
    private int _count = 1;

    public TokenizerActivatorService(ITokenizerService tokenizer, ILogger<TokenizerActivatorService> logger)
    {
        _tokenizer = tokenizer;
        _logger = logger;
    }

    /// <summary>
    /// Инициализация токенайзера по расписанию
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("[TokenizerActivatorService] is active, prepare to runs for '{Count}' time", _count);

                _count++;

                _tokenizer.Initialize();

                await Task.Delay(WaitMs, stoppingToken);
            }
        }
        finally
        {
            _logger.LogInformation("[{Name}] graceful shutdown after '{Count}' cycles", nameof(TokenizerActivatorService), _count.ToString());
        }
    }
}
