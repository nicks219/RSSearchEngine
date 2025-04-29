using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Microsoft.EntityFrameworkCore.Query;

namespace SearchEngine.Tests.Units.Mocks.DatabaseRepo;

/// <summary>
/// Мок функционала EF для юнит-тестов
/// </summary>
/// <typeparam name="T"></typeparam>
public class FakeDbSet<T> : IAsyncEnumerable<T>, IOrderedQueryable<T>
{
    private readonly List<T> _data;

    public FakeDbSet(IEnumerable<T> data)
    {
        _data = data.ToList();
        Provider = _data.AsQueryable().Provider;
        Expression = _data.AsQueryable().Expression;
        ElementType = _data.AsQueryable().ElementType;
    }

    public FakeDbSet(IEnumerable<T> data, IAsyncQueryProvider provider)
    {
        _data = data.ToList();
        Provider = provider;
        Expression = _data.AsQueryable().Expression;
        ElementType = _data.AsQueryable().ElementType;
    }

    public IEnumerator<T> GetEnumerator() => _data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IQueryProvider Provider { get; }
    public Expression Expression { get; }
    public Type ElementType { get; }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken ct = default) => _data.ToAsyncEnumerable().GetAsyncEnumerator(ct);
}
