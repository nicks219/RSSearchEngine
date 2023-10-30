using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SearchEngine.Infrastructure.Tokenizer.Contracts;

namespace SearchEngine.Infrastructure;

public class TokenizerActivatorService : BackgroundService
{
    private const int BaseMs = 1000;
    private const int Min = 60;
    private const int Hour = 60;
    private const int Day = 24;
    // update раз в сутки
    private const int TimeWait = 1 * Day * Hour * Min * BaseMs;
    private readonly ITokenizerService _cache;
    private readonly ILogger<TokenizerActivatorService> _logger;
    private int _count;

    public TokenizerActivatorService(ITokenizerService cache, ILogger<TokenizerActivatorService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("[cache activator service] activate {Count}", _count);

                // для MySql отслеживание реализуется через триггеры:
                // CREATE TRIGGER articles_log_update AFTER update ON articles
                // FOR EACH ROW BEGIN
                // INSERT articles_logs (action, row_id, `date`) VALUES ("update", new.id, NOW());
                // END

                _count++;

                _cache.Initialize();

                await Task.Delay(TimeWait, stoppingToken);
            }
        }
        finally
        {
            _logger.LogInformation("[cache activator service] graceful shutdown after {Count} cycles", _count.ToString());
        }
    }
}
