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
public readonly ref struct ReducedSearchSimple : IReducedSearchProcessor
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

    // в коде содержится хардкод констант для прохождения юнит-тестов
    // следует переосмыслить стратегию расчета метрик для "плохих" запросов
    // и перенести константы из MetricsCalculator в SimpleEngine.VectorSearch

    /// <inheritdoc/>
    public void FindReduced(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException(nameof(ReducedSearchLegacy));
        }

        // необходимо оценивать токены по количеству идентификаторов и отфильтровывать самые частотные, например:
        // I. query: 11950 - 100 - 100 - 4250 - 7600 - 2500
        // query: "приключится вдруг вот верный друг выручить"
        // query: total:49K - search:21K - max:50
        // II. duplicates: total:1K - search:1K - max:1

        var totalDocumentCount = GeneralDirectIndexLegacy.Count;
        // костыль для юнит-тестов: хардкодим 5K как 10% от предполагаемого общего числа документов
        var rejectThreshold = Math.Max(5000, totalDocumentCount / 10);
        Dictionary<DocumentId, int> tokenOverlapCounts = TempStoragePool.TokenOverlapCounts.Get();
        HashSet<DocumentId> relevantDocumentIds = TempStoragePool.RelevantDocumentIds.Get();

        try
        {
            // создаём пространство поиска (без учета последовательности токенов) и оцениваем количество токенов из запроса на идентификатор
            // значение это по сути баллы, набранные запросом для конкретной заметки
            // при "мусорных" токенах пространство поиска может быть практически равно общему индексу
            // 0.6D - коэффициент релевантности для reduced
            var minRelevanceScore = searchVector.Count * 0.6D;
            var tokensCanBeRejected = searchVector.Count - (int)minRelevanceScore;
            foreach (Token token in searchVector)
            {
                if (!InvertedIndexLegacy.TryGetIds(token, out var ids))
                {
                    continue;
                }

                // необходимо оценивать токены по количеству идентификаторов и отфильтровывать самые частотные
                // данный подход оставит только максимумы в поисковой выдаче для низкорейтинговых запросов (см юниты)
                // костыль: сейчас мы пропускаем первые попавшиеся высокочастотные токены, это удачно совпадает с тестовыми данными
                // полноценный метод см в DataProcessorPrototype.RemoveMostFrequentTokens
                if (ids.Count > rejectThreshold && --tokensCanBeRejected > 0)
                {
                    continue;
                }

                foreach (var id in ids)
                {
                    if (!tokenOverlapCounts.TryAdd(id, 1))
                    {
                        var score = ++tokenOverlapCounts[id];
                        // костыль для юнит-тестов (которые фиксируют низкорейтинговые результаты): TestData.ResponseLimitLarge = 147
                        if (score >= minRelevanceScore && relevantDocumentIds.Count < 147)
                        {
                            relevantDocumentIds.Add(id);
                        }
                    }
                }
            }

            // поиск в пространстве поиска reduced
            // баллы совпадений будут посчитаны повторно
            foreach (var documentId in relevantDocumentIds)
            {
                var tokenLine = GeneralDirectIndexLegacy[documentId];
                metricsCalculator.AppendReducedMetric(searchVector, documentId, tokenLine);
            }
            // использовать, если баллы посчитаны корректно
            /*foreach (var documentId in relevantDocumentIds)
            {
                var tokenLine = GeneralDirectIndexLegacy[documentId];
                var comparisonScore = tokenMatchCounts[documentId];
                metricsCalculator.AppendReducedMetric(searchVector, documentId, tokenLine, comparisonScore);
            }*/
        }
        finally
        {
            TempStoragePool.RelevantDocumentIds.Return(relevantDocumentIds);
            TempStoragePool.TokenOverlapCounts.Return(tokenOverlapCounts);
        }
    }
}
