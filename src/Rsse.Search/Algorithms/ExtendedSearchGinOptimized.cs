using System;
using System.Collections.Generic;
using System.Threading;
using Rsse.Search.Dto;
using Rsse.Search.Indexes;
using Rsse.Search.Processor;

namespace Rsse.Search.Algorithms;

/// <summary>
/// Класс с алгоритмом подсчёта расширенной метрики.
/// Пространство поиска формируется с помощью GIN индекса.
/// </summary>
public sealed class ExtendedSearchGinOptimized : IExtendedSearchProcessor
{
    /// <summary>
    /// Индекс для всех токенизированных заметок.
    /// </summary>
    public required DirectIndex GeneralDirectIndex { get; init; }

    /// <summary>
    /// Поддержка GIN-индекса.
    /// </summary>
    public required InverseIndex<DocumentIdSet> GinExtended { get; init; }

    /// <inheritdoc/>
    public void FindExtended(TokenVector searchVector, IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        // выбрать только те заметки, которые пригодны для extended поиска
        var idsExtendedSearchSpace = new HashSet<DocumentId>();
        foreach (var token in searchVector)
        {
            if (!GinExtended.TryGetIdentifiers(token, out var docIds))
            {
                continue;
            }

            // если в gin есть токен, перебираем id заметок в которых он присутствует и формируем пространство поиска
            foreach (var docId in docIds)
            {
                // выигрывает по нагрузке на GC:
                // if (!tokenLinesExtendedSearchSpace.TryGetValue(docId, out var tokenLine)) {
                // var originalTokenLine = GeneralDirectIndex[docId];
                // tokenLinesExtendedSearchSpace[docId] = originalTokenLine with { Reduced = emptyReducedVector };

                idsExtendedSearchSpace.Add(docId);
            }
        }

        if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(nameof(ExtendedSearchGinOptimized));
        foreach (var docId in idsExtendedSearchSpace)
        {
            var extendedTargetVector = GeneralDirectIndex[docId].Extended;
            var comparisonScore = ScoreCalculator.ComputeOrdered(extendedTargetVector, searchVector);

            // Для расчета метрик необходимо учитывать размер оригинальной заметки.
            metricsCalculator.AppendExtended(comparisonScore, searchVector, docId, extendedTargetVector);
        }
    }
}
