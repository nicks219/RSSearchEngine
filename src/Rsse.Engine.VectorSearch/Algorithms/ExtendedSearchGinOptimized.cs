using System;
using System.Collections.Generic;
using System.Threading;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.Processor;

namespace RsseEngine.Algorithms;

/// <summary>
/// Класс с алгоритмом подсчёта расширенной метрики.
/// Пространство поиска формируется с помощью GIN индекса.
/// </summary>
public sealed class ExtendedSearchGinOptimized : IExtendedSearchProcessor
{
    /// <summary>
    /// Общий индекс: идентификатор-вектор.
    /// </summary>
    public required DirectIndex GeneralDirectIndex { get; init; }

    /// <summary>
    /// Общий инвертированный индекс: токен-идентификаторы.
    /// </summary>
    public required InvertedIndex<DocumentIdSet> GinExtended { get; init; }

    public required GinRelevanceFilter RelevanceFilter { get; init; }

    /// <inheritdoc/>
    public void FindExtended(TokenVector searchVector, IMetricsCalculator metricsCalculator, CancellationToken cancellationToken)
    {
        if (!RelevanceFilter.Enabled)
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
        else
        {
            var filteredDocuments = RelevanceFilter.ProcessToSet(GinExtended, searchVector);

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
                    if (!filteredDocuments.Contains(docId))
                    {
                        continue;
                    }

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
}
