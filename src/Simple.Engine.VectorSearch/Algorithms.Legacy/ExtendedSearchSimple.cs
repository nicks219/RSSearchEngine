using System;
using System.Collections.Generic;
using System.Threading;
using SimpleEngine.Contracts;
using SimpleEngine.Dto.Common;
using SimpleEngine.Indexes;
using SimpleEngine.Pools;
using SimpleEngine.Processor;

namespace SimpleEngine.Algorithms.Legacy;

/// <summary>
/// "Оригинальный" алгоритм с дополнительным инвертированным индексом.
/// </summary>
public readonly ref struct ExtendedSearchSimple : IExtendedSearchProcessor
{
    /// <summary>
    /// Общий индекс: идентификатор - вектор.
    /// </summary>
    public required GeneralDirectIndexLegacy GeneralDirectIndexLegacy { private get; init; }

    /// <summary>
    /// Обратный индекс: токен - идентификаторы.
    /// </summary>
    public required InvertedIndexLegacy InvertedIndexLegacy { private get; init; }

    /// <summary>
    /// Пул для коллекций.
    /// </summary>
    public required TempStoragePool TempStoragePool { private get; init; }

    /// <inheritdoc/>
    public void FindExtended(TokenVector searchVector, IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException(nameof(ExtendedSearchLegacy));
        }

        // создаём пространство поиска (без учета последовательности токенов)
        // значение это по сути баллы, набранные запросом для конкретной заметке
        // при "мусорных" токенах пространство поиска может быть практически равно общему индексу, поэтому считаем статистику
        Dictionary<DocumentId, int> searchSpace = [];
        foreach (Token token in searchVector)
        {
            if (!InvertedIndexLegacy.TryGetValue(token, out var ids))
            {
                continue;
            }

            foreach (var id in ids)
            {
                if (!searchSpace.TryAdd(id, 1))
                {
                    searchSpace[id]++;
                    // можно сразу посчитать максимум
                }
            }
        }

        // сортируем:
        // var sortedSearchSpace = searchSpace.OrderByDescending(x => x.Value);
        // выбираем результат(ы) с одинаковым максимальным рейтингом (у них не обязательно самая высокая релевантность):
        var max = int.MinValue;
        var searchSpaceMax = new List<DocumentId>();
        foreach (var kv in searchSpace)
        {
            if (kv.Value > max)
            {
                max = kv.Value;
                searchSpaceMax.Clear();
                searchSpaceMax.Add(kv.Key);// = kv.Value;
            }
            else if (kv.Value == max)
            {
                searchSpaceMax.Add(kv.Key);// = kv.Value;
            }
        }

        // поиск в пространстве поиска extended
        // баллы совпадений будут посчитаны повторно, но с учетом последовательности токенов
        foreach (var documentId in searchSpaceMax)
        {
            var tokenLine = GeneralDirectIndexLegacy[documentId];
            metricsCalculator.AppendExtendedMetric(searchVector, documentId, tokenLine);
        }
    }
}
