using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SearchEngine.Tests.Infrastructure.DAL;

public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly Task<IEnumerable<T>> _enumerableTask;
    private IEnumerator<T>? _enumerator;

    public TestAsyncEnumerator(Task<IEnumerable<T>> enumerableTask)
    {
        _enumerableTask = enumerableTask;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        _enumerator ??= _enumerableTask.Result.GetEnumerator();
        return new ValueTask<bool>(_enumerator.MoveNext());
    }

    public T Current => _enumerator!.Current;

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        _enumerator?.Dispose();
        return new ValueTask();
    }
}
