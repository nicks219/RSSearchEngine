using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SearchEngine.Tests.Units.Mocks.DatabaseRepo;

// todo: избавиться от этого мока и всего связанного содержимого в неймспейсе
internal class TestAsyncEnumerator<T>(Task<IEnumerable<T>> enumerableTask) : IAsyncEnumerator<T>
{
    private IEnumerator<T>? _enumerator;

    public ValueTask<bool> MoveNextAsync()
    {
        _enumerator ??= enumerableTask.Result.GetEnumerator();
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
