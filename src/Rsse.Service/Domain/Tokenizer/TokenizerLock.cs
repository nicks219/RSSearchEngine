using System;
using System.Threading;
using System.Threading.Tasks;

namespace SearchEngine.Domain.Tokenizer;

/// <summary>
/// Блокировка в disposable-обертке
/// </summary>
public sealed class TokenizerLock : IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary/> Эксклюзивная блокировка
    public async Task<ExclusiveLockToken> AcquireExclusiveLockAsync()
    {
        await _semaphore.WaitAsync();
        return new ExclusiveLockToken(_semaphore);
    }

    /// <summary/> Дождаться блокировку и сразу отдать её
    public async Task SyncOnLockAsync()
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);
        _semaphore.Release();
    }

    /// <summary/> Получить блокировку в disposable-обёртке
    public readonly struct ExclusiveLockToken(SemaphoreSlim semaphore) : IDisposable
    {
        public void Dispose() => semaphore.Release();
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }
}
