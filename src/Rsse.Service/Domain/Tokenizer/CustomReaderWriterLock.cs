using System;
using System.Threading;

namespace SearchEngine.Domain.Tokenizer;

/// <summary>
/// Блокировка в disposable-обертке
/// </summary>
public class CustomReaderWriterLock : IDisposable
{
    private readonly ReaderWriterLockSlim _lock = new();

    public ReadLockToken ReadLock() => new(_lock);
    public WriteLockToken WriteLock() => new(_lock);

    public readonly struct WriteLockToken : IDisposable
    {
        private readonly ReaderWriterLockSlim _lock;
        public WriteLockToken(ReaderWriterLockSlim rwLock)
        {
            _lock = rwLock;
            rwLock.EnterWriteLock();
        }
        public void Dispose() => _lock.ExitWriteLock();
    }

    public readonly struct ReadLockToken : IDisposable
    {
        private readonly ReaderWriterLockSlim _lock;
        public ReadLockToken(ReaderWriterLockSlim rwLock)
        {
            _lock = rwLock;
            _lock.EnterReadLock();
        }
        public void Dispose() => _lock.ExitReadLock();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _lock.Dispose();
    }
}
