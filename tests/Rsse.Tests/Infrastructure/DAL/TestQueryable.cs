using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

namespace SearchEngine.Tests.Infrastructure.DAL;

public class TestQueryable<T> : IAsyncEnumerable<T>, IOrderedQueryable<T>
{
    private readonly IAsyncQueryProvider _queryProvider;

    internal TestQueryable(IAsyncQueryProvider queryProvider, Expression expression)
    {
        _queryProvider = queryProvider;
        Expression = expression;
    }

    public TestQueryable(IEnumerable<T> enumerable) : this(new TestQueryProvider(), Expression.Constant(enumerable.AsQueryable()))
    {
    }

    public IEnumerator<T> GetEnumerator()
    {
        var enumerator = _queryProvider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        return enumerator;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    IAsyncEnumerator<T> IAsyncEnumerable<T>.GetAsyncEnumerator(CancellationToken cancellationToken)
    {
        var enumerable = Task.FromResult(_queryProvider.ExecuteAsync<IEnumerable<T>>(Expression, cancellationToken));
        var enumerator = new TestAsyncEnumerator<T>(enumerable);
        return enumerator;
    }

    public Type ElementType => typeof(T);
    public Expression Expression { get; }

    public IQueryProvider Provider => _queryProvider;
}
