using System;
using System.Threading;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;
using RsseEngine.Pools;
using RsseEngine.Processor;

namespace RsseEngine.Algorithms;

/// <summary>
/// Класс с алгоритмом подсчёта расширенной метрики.
/// Пространство поиска формируется с помощью GIN индекса.
/// </summary>
public sealed class ExtendedSearchGinOptimized : IExtendedSearchProcessor
{
    public required TempStoragePool TempStoragePool { private get; init; }

    /// <summary>
    /// Общий индекс: идентификатор-вектор.
    /// </summary>
    public required DirectIndex GeneralDirectIndex { private get; init; }

    /// <summary>
    /// Общий инвертированный индекс: токен-идентификаторы.
    /// </summary>
    public required InvertedIndex<DocumentIdSet> GinExtended { private get; init; }

    /// <inheritdoc/>
    public void FindExtended(TokenVector searchVector, IMetricsCalculator metricsCalculator,
        CancellationToken cancellationToken)
    {
        var idsExtendedSearchSpace = TempStoragePool.SetsTempStorage.Get();
        var tokens = TempStoragePool.TokenSetsTempStorage.Get();

        try
        {
            // выбрать только те заметки, которые пригодны для extended поиска
            foreach (var token in searchVector)
            {
                if (!GinExtended.TryGetNonEmptyDocumentIdVector(token, out var docIds))
                {
                    continue;
                }

                if (!tokens.Add(token))
                {
                    continue;
                }

                // если в gin есть токен, перебираем id заметок в которых он присутствует и формируем пространство поиска
                foreach (var documentId in docIds)
                {
                    idsExtendedSearchSpace.Add(documentId);
                }
            }

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(nameof(ExtendedSearchGinOptimized));

            foreach (var documentId in idsExtendedSearchSpace)
            {
                var extendedTargetVector = GeneralDirectIndex[documentId].Extended;
                var comparisonScore = ScoreCalculator.ComputeOrdered(extendedTargetVector, searchVector);

                // Для расчета метрик необходимо учитывать размер оригинальной заметки.
                metricsCalculator.AppendExtended(comparisonScore, searchVector, documentId, extendedTargetVector);
            }
        }
        finally
        {
            TempStoragePool.TokenSetsTempStorage.Return(tokens);
            TempStoragePool.SetsTempStorage.Return(idsExtendedSearchSpace);
        }
    }
}
