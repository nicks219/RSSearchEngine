using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Infrastructure.Cache.Contracts;
using SearchEngine.Infrastructure.Engine;
using SearchEngine.Infrastructure.Engine.Contracts;

namespace SearchEngine.Service.Models;

public class FindModel
{
    private readonly IDataRepository _repo;
    private readonly ITextProcessor _processor;

    private readonly ConcurrentDictionary<int, List<int>> _undefinedCache;
    private readonly ConcurrentDictionary<int, List<int>> _definedCache;

    public FindModel(IServiceScope scope)
    {
        _repo = scope.ServiceProvider.GetRequiredService<IDataRepository>();
        _processor = scope.ServiceProvider.GetRequiredService<ITextProcessor>();

        _undefinedCache = scope.ServiceProvider.GetRequiredService<ICacheRepository>().GetUndefinedCache();
        _definedCache = scope.ServiceProvider.GetRequiredService<ICacheRepository>().GetDefinedCache();
    }

    public int FindIdByName(string name)
    {
        var id = _repo.FindNoteIdByTitle(name);

        return id;
    }

    public Dictionary<int, double> Find(string text)
    {
        var result = new Dictionary<int, double>();

        // I. defined поиск: 0.8D
        const double defined = 0.8D;
        // II. undefined поиск: 0.4D
        const double undefined = 0.6D; // 0.6 .. 0.75

        var undefinedSearch = true;

        _processor.Setup(ConsonantChain.Defined);

        var hash = _processor.CleanUpString(text);

        if (hash.Count == 0)
        {
            // песни вида "123 456" не ищем, так как получим весь каталог
            return result;
        }

        var item = _processor.GetHashSetFromStrings(hash);

        foreach (var (key, value) in _definedCache)
        {
            var metric = _processor.GetComparisionMetric(value, item);

            // I. 100% совпадение defined, undefined можно не искать
            if (metric == item.Count)
            {
                undefinedSearch = false;
                result.Add(key, metric * (1000D / value.Count)); // было int
                continue;
            }

            // II. defined% совпадение
            if (metric >= item.Count * defined)
            {
                // [TODO] можно так оценить
                // undefinedSearch = false;
                result.Add(key, metric * (100D / value.Count)); // было int
            }
        }

        if (!undefinedSearch)
        {
            return result;
        }

        _processor.Setup(ConsonantChain.Undefined);

        hash = _processor.CleanUpString(text);

        item = _processor.GetHashSetFromStrings(hash);

        if (hash.Count == 0)
        {
            // песни вида "123 456" не ищем, так как получим весь каталог
            return result;
        }

        // убираем дубликаты слов для intersect - это меняет результаты поиска
        item = item.ToHashSet().ToList();

        foreach (var (key, value) in _undefinedCache)
        {
            var metric = _processor.GetComparisionMetric(value, item);

            // III. 100% совпадение undefined
            if (metric == item.Count)
            {
                result.TryAdd(key, metric * (10D / value.Count)); // было int
                continue;
            }

            // IV. undefined% совпадение - мы не можем наверняка оценить неточное совпадение
            if (metric >= item.Count * undefined)
            {
                result.TryAdd(key, metric * (1D / value.Count)); // было int
            }
        }

        return result;
    }
}
