using System;
using System.Threading;
using System.Threading.Tasks;

namespace SearchEngine.Service.Tokenizer;

/// <summary>
/// Блокировка в disposable-обертке.
/// </summary>
public sealed class TokenizerLock : IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary/> Эксклюзивная блокировка.
    public async Task<ExclusiveLockToken> AcquireExclusiveLockAsync(CancellationToken stoppingToken)
    {
        await _semaphore.WaitAsync(stoppingToken);
        return new ExclusiveLockToken(_semaphore);
    }

    /// <summary/> Дождаться блокировку и сразу отдать её.
    public async Task SyncOnLockAsync(CancellationToken timeoutToken)
    {
        await _semaphore.WaitAsync(timeoutToken);
        _semaphore.Release();
    }

    /// <summary/> Получить блокировку в disposable-обёртке.
    public readonly struct ExclusiveLockToken(SemaphoreSlim semaphore) : IDisposable
    {
        public void Dispose() => semaphore.Release();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _semaphore.Dispose();
    }
}
