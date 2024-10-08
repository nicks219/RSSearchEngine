using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Engine.Contracts;
using SearchEngine.Engine.Tokenizer;

namespace SearchEngine.Models;

/// <summary>
/// Функционал поиска заметок
/// </summary>
public class CompliantModel(IServiceScope scope)
{
    private readonly IDataRepository _repo = scope.ServiceProvider.GetRequiredService<IDataRepository>();
    private readonly ITokenizerProcessor _processor = scope.ServiceProvider.GetRequiredService<ITokenizerProcessor>();

    private readonly ConcurrentDictionary<int, List<int>> _reducedLines = scope.ServiceProvider.GetRequiredService<ITokenizerService>().GetReducedLines();
    private readonly ConcurrentDictionary<int, List<int>> _extendedLines = scope.ServiceProvider.GetRequiredService<ITokenizerService>().GetExtendedLines();

    /// <summary>
    /// Найти идентификатор заметки по её имени
    /// </summary>
    /// <param name="name">имя заметки</param>
    /// <returns>идентификатор заметки</returns>
    public int FindNoteId(string name)
    {
        var id = _repo.ReadNoteId(name);

        return id;
    }

    /// <summary>
    /// Вычислить индексы соответсвия хранимых заметок поисковому запросу
    /// </summary>
    /// <param name="text">текст для поиска соответствий</param>
    /// <returns>идентификаторы заметок и их индексы соответствия</returns>
    public Dictionary<int, double> ComputeComplianceIndices(string text)
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
