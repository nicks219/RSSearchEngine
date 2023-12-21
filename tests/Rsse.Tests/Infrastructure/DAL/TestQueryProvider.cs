using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using SearchEngine.Data.Entities;

namespace SearchEngine.Tests.Infrastructure.DAL;

public class TestQueryProvider : IAsyncQueryProvider
{
    public IQueryable CreateQuery(Expression expression)
    {
        var elementType = GetElementType(expression.Type);
        var queryType = typeof(TestQueryable<>).MakeGenericType(elementType);
        if (queryType == null)
        {
            throw new Exception("Null query type");
        }

        if (Activator.CreateInstance(queryType, this, expression) is not IQueryable queryable)
        {
            throw new Exception("Null queryable");
        }

        return queryable;
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestQueryable<TElement>(this, expression);

    // нуждается в правке:
    public object Execute(Expression expression) => new List<int> { 10, 20, 30 };

    public TResult Execute<TResult>(Expression expression)
    {
        var type = typeof(TResult);

        // cache tests:
        if (type == typeof(IEnumerable<NoteEntity>))
        {
            if (expression is ConstantExpression constantExpression)
            {
                var value = constantExpression.Value;
                return value is not null ? (TResult)value : throw new Exception("HERE!");
            }
        }

        // catalog tests:
        if (type == typeof(int) && expression is MethodCallExpression callExpression)
        {
            var query = (callExpression.Arguments[0] as ConstantExpression)?.Value;
            var enumerableQuery = query as EnumerableQuery<NoteEntity>;
            var count = enumerableQuery?.Count() ?? throw new NullReferenceException("Count should not be null");
            return (TResult)(object)count;
        }

        return (TResult)(object)new List<int> { 101, 201, 301 };
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = new())
    {
        var type = typeof(TResult);

        switch (expression.NodeType)
        {
            // I. вызов из TestRepo: public IQueryable<int> ReadAllNotesTaggedBy(IEnumerable<int> checkedTags):
            // TResult: Task<int>: сигнатура ElectNoteId, стаб на таску
            case ExpressionType.Call when type.BaseType?.Name == nameof(Task):
                {
                    var firstArgument = ((MethodCallExpression)expression).Arguments[0];
                    if (firstArgument.NodeType == ExpressionType.Constant)
                    {
                        // A. выполнение: await allElectableNotes.CountAsync();
                        var value = ((ConstantExpression)firstArgument).Value;
                        if (value == null) throw new NullReferenceException(nameof(value) + "is null");
                        return ((EnumerableQuery<int>)value).Count() > 1
                            // добавил для теста на гистограмму:
                            ? (TResult)(object)Task.FromResult(200)
                            : (TResult)(object)Task.FromResult(((EnumerableQuery<int>)value).ElementAt(0));
                    }
                    else
                    {
                        // B. выполнение: orderby>skip(...)>take(1)>firstasync(): В СКИПЕ НУЖНОЕ МНЕ ЧИСЛО !
                        firstArgument = ((MethodCallExpression)firstArgument).Arguments[0];
                        var secondArgument = ((MethodCallExpression)firstArgument).Arguments[1];
                        var value = ((ConstantExpression)secondArgument).Value;
                        if (value == null) throw new NullReferenceException(nameof(value) + "is null");
                        var element = (int)value;
                        var result = (TResult)(object)Task.FromResult(element);
                        return result;
                    }
                }

            // II. public IQueryable<Tuple<string, string>> ReadNote(int noteId):
            // TResult: EnumerableQuery<Tuple<string, string>>, это ReadNote из репо, асинхронное перечисление
            case ExpressionType.Constant:
                {
                    var value = (ConstantExpression)expression;
                    if (value.Value?.GetType() == typeof(EnumerableQuery<Tuple<string, string>>))
                    {
                        var values = (EnumerableQuery<Tuple<string, string>>)value.Value;
                        var result = (TResult)(object)values;
                        return result;
                    }

                    // update: .ReadNoteTags(originalNoteId).ToListAsync()..
                    if (value.Value?.GetType() == typeof(EnumerableQuery<int>))
                    {
                        var values = (EnumerableQuery<int>)value.Value;
                        var result = (TResult)(object)values;
                        return result;
                    }

                    // catalog: await _repo.ReadCatalogPage..
                    if (value.Value?.GetType() == typeof(EnumerableQuery<Tuple<string, int>>))
                    {
                        var values = (EnumerableQuery<Tuple<string, int>>)value.Value;
                        var result = (TResult)(object)values;
                        return result;
                    }

                    break;
                }

            default: throw new NotImplementedException("Unknown node type");
        }

        throw new NotImplementedException("Unknown expression type");
    }

    private static Type GetElementType(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IQueryable<>)
            ? type.GetGenericArguments()[0]
            : type;
    }
}
