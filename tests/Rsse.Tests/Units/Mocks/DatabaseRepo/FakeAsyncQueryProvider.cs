using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

namespace SearchEngine.Tests.Units.Mocks.DatabaseRepo;

// todo: избавиться от мока и связанного содержимого в неймспейсе, использовать стандартный вариант
// NB: мок требуется методаIQueryable<int> ReadTaggedNotes(IEnumerable<int> checkedTags) из репо
// NB: см. Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
internal class FakeAsyncQueryProvider : IAsyncQueryProvider
{
    public IQueryable CreateQuery(Expression expression) => throw new NotImplementedException();

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new FakeDbSet<TElement>(expression, this);

    public object Execute(Expression expression) => throw new NotImplementedException();

    public TResult Execute<TResult>(Expression expression) => throw new NotImplementedException();

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = new())
    {
        var type = typeof(TResult);

        switch (expression.NodeType)
        {
            // I. TestRepo: public IQueryable<int> ReadAllNotesTaggedBy(IEnumerable<int> checkedTags)..
            // TResult: Task<int>..
            case ExpressionType.Call when type.BaseType?.Name == nameof(Task):
                {
                    var firstArgument = ((MethodCallExpression)expression).Arguments[0];
                    if (firstArgument.NodeType == ExpressionType.Constant)
                    {
                        // A. await allElectableNotes.CountAsync()..
                        var value = ((ConstantExpression)firstArgument).Value;
                        if (value == null) throw new NullReferenceException(nameof(value) + "is null");

                        // histogram test:
                        if (((EnumerableQuery<int>) value).Count() > 1) return (TResult)(object)Task.FromResult(200);

                        var result = ((EnumerableQuery<int>) value).ElementAt(0);
                        return (TResult)(object)Task.FromResult(result);
                    }
                    else
                    {
                        // B. OrderBy -> skip(...) -> take(1) -> FirstAsync()..
                        firstArgument = ((MethodCallExpression)firstArgument).Arguments[0];
                        var secondArgument = ((MethodCallExpression)firstArgument).Arguments[1];
                        var value = ((ConstantExpression)secondArgument).Value;
                        if (value == null) throw new NullReferenceException(nameof(value) + "is null");

                        var element = (int)value;
                        var result = (TResult)(object)Task.FromResult(element);
                        return result;
                    }
                }

            default: throw new NotImplementedException("unknown node type");
        }
    }
}
