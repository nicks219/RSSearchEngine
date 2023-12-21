using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Infrastructure.Tokenizer;
using SearchEngine.Infrastructure.Tokenizer.Contracts;

namespace SearchEngine.Service.Models;

public class FindModel
{
    private readonly IDataRepository _repo;
    private readonly ITokenizerProcessor _processor;

    private readonly ConcurrentDictionary<int, List<int>> _reducedLines;
    private readonly ConcurrentDictionary<int, List<int>> _extendedLines;

    public FindModel(IServiceScope scope)
    {
        _repo = scope.ServiceProvider.GetRequiredService<IDataRepository>();
        _processor = scope.ServiceProvider.GetRequiredService<ITokenizerProcessor>();

        _reducedLines = scope.ServiceProvider.GetRequiredService<ITokenizerService>().GetReducedLines();
        _extendedLines = scope.ServiceProvider.GetRequiredService<ITokenizerService>().GetExtendedLines();
    }

    public int FindNoteId(string name)
    {
        var id = _repo.ReadNoteId(name);

        return id;
    }

    public Dictionary<int, double> ComputeSearchIndexes(string text)
    {
        var result = new Dictionary<int, double>();

        // I. коэффициент extended поиска: 0.8D
        const double extended = 0.8D;
        // II. коэффициент reduced поиска: 0.4D
        const double reduced = 0.6D; // 0.6 .. 0.75

        var reducedChainSearch = true;

        _processor.SetupChain(ConsonantChain.Extended);

        var preprocessedStrings = _processor.PreProcessNote(text);

        if (preprocessedStrings.Count == 0)
        {
            // заметки вида "123 456" не ищем, так как получим весь каталог
            return result;
        }

        var newTokensLine = _processor.TokenizeSequence(preprocessedStrings);

        foreach (var (key, cachedTokensLine) in _extendedLines)
        {
            var metric = _processor.ComputeComparisionMetric(cachedTokensLine, newTokensLine);

            // I. 100% совпадение по extended последовательности, по reduced можно не искать
            if (metric == newTokensLine.Count)
            {
                reducedChainSearch = false;
                result.Add(key, metric * (1000D / cachedTokensLine.Count)); // было int
                continue;
            }

            // II. extended% совпадение
            if (metric >= newTokensLine.Count * extended)
            {
                // [TODO] можно так оценить
                // reducedChainSearch = false;
                result.Add(key, metric * (100D / cachedTokensLine.Count)); // было int
            }
        }

        if (!reducedChainSearch)
        {
            return result;
        }

        _processor.SetupChain(ConsonantChain.Reduced);

        preprocessedStrings = _processor.PreProcessNote(text);

        newTokensLine = _processor.TokenizeSequence(preprocessedStrings);

        if (preprocessedStrings.Count == 0)
        {
            // песни вида "123 456" не ищем, так как получим весь каталог
            return result;
        }

        // убираем дубликаты слов для intersect - это меняет результаты поиска
        newTokensLine = newTokensLine.ToHashSet().ToList();

        foreach (var (key, cachedTokensLine) in _reducedLines)
        {
            var metric = _processor.ComputeComparisionMetric(cachedTokensLine, newTokensLine);

            // III. 100% совпадение по reduced
            if (metric == newTokensLine.Count)
            {
                result.TryAdd(key, metric * (10D / cachedTokensLine.Count)); // было int
                continue;
            }

            // IV. reduced% совпадение - мы не можем наверняка оценить неточное совпадение
            if (metric >= newTokensLine.Count * reduced)
            {
                result.TryAdd(key, metric * (1D / cachedTokensLine.Count)); // было int
            }
        }

        return result;
    }
}
