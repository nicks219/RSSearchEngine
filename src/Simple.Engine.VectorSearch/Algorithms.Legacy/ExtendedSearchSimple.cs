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

        Dictionary<DocumentId, int> tokenOverlapCounts = TempStoragePool.TokenOverlapCounts.Get();
        HashSet<DocumentId> relevantDocumentIds = TempStoragePool.RelevantDocumentIds.Get();

        try
        {
            // создаём пространство поиска (без учета последовательности токенов) и считаем количество токенов из запроса на идентификатор
            // значение это по сути баллы, набранные запросом для конкретной заметки
            // при "мусорных" токенах пространство поиска может быть практически равно общему индексу

            foreach (Token token in searchVector)
            {
                if (!InvertedIndexLegacy.TryGetValue(token, out var ids))
                {
                    continue;
                }

                foreach (var id in ids)
                {
                    if (!tokenOverlapCounts.TryAdd(id, 1))
                    {
                        tokenOverlapCounts[id]++;
                        // todo: можно сразу считать максимум, но прироста не даёт, исследуй
                    }
                }
            }

            // выбираем результат(ы) с одинаковым максимальным рейтингом (у них не обязательно самая высокая релевантность):
            var max = int.MinValue;
            foreach (var kv in tokenOverlapCounts)
            {
                if (kv.Value > max)
                {
                    max = kv.Value;
                    relevantDocumentIds.Clear();
                    relevantDocumentIds.Add(kv.Key);
                }
                else if (kv.Value == max)
                {
                    relevantDocumentIds.Add(kv.Key);
                }
            }

            // поиск в пространстве поиска extended
            // баллы совпадений будут посчитаны повторно, но с учетом последовательности токенов
            foreach (var documentId in relevantDocumentIds)
            {
                var tokenLine = GeneralDirectIndexLegacy[documentId];
                metricsCalculator.AppendExtendedMetric(searchVector, documentId, tokenLine);
            }
        }
        finally
        {
            TempStoragePool.RelevantDocumentIds.Return(relevantDocumentIds);
            TempStoragePool.TokenOverlapCounts.Return(tokenOverlapCounts);
        }
    }
}
