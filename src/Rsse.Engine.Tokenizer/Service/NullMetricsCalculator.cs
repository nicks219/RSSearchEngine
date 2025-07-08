using System.Collections.Generic;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;

namespace RsseEngine.Service;

/// <summary>
/// Фиктивный функционал подсчета метрик релевантности, не аллоцирует коллекцию с метриками.
/// </summary>
internal sealed class NullMetricsCalculator : IMetricsCalculator
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public DocumentId DocumentId { get; private set; }

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public double Metric { get; private set; }

    /// <inheritdoc/>
    public bool ContinueSearching { get; private set; } = true;

    /// <inheritdoc/>
    public Dictionary<DocumentId, double> ComplianceMetrics { get; } = new()
    {
        {
            new DocumentId(1), 0
        }
    };

    /// <inheritdoc/>
    public void AppendExtended(int comparisonScore, TokenVector searchVector, DocumentId documentId,
        int extendedTargetVectorSize)
    {
        // I. 100% совпадение по extended последовательности, по reduced можно не искать
        if (comparisonScore == searchVector.Count)
        {
            ContinueSearching = false;
            DocumentId = documentId;
            Metric = comparisonScore * (1000D / extendedTargetVectorSize);
            return;
        }

        // II. extended% совпадение
        if (comparisonScore >= searchVector.Count * MetricsCalculator.ExtendedCoefficient)
        {
            DocumentId = documentId;
            // todo: можно так оценить
            // continueSearching = false;
            Metric = comparisonScore * (100D / extendedTargetVectorSize);
        }
    }

    public void AppendExtended(int comparisonScore, TokenVector searchVector, DocumentId documentId, DirectIndex directIndex)
    {
        // I. 100% совпадение по extended последовательности, по reduced можно не искать
        if (comparisonScore == searchVector.Count)
        {
            var extendedTargetVector = directIndex[documentId].Extended;
            ContinueSearching = false;
            DocumentId = documentId;
            Metric = comparisonScore * (1000D / extendedTargetVector.Count);
            return;
        }

        // II. extended% совпадение
        if (comparisonScore >= searchVector.Count * MetricsCalculator.ExtendedCoefficient)
        {
            var extendedTargetVector = directIndex[documentId].Extended;
            DocumentId = documentId;
            // todo: можно так оценить
            // continueSearching = false;
            Metric = comparisonScore * (100D / extendedTargetVector.Count);
        }
    }

    /// <inheritdoc/>
    public void AppendReduced(int comparisonScore, TokenVector searchVector, DocumentId documentId,
        int reducedTargetVectorSize)
    {
        // III. 100% совпадение по reduced
        if (comparisonScore == searchVector.Count)
        {
            DocumentId = documentId;
            Metric = comparisonScore * (10D / reducedTargetVectorSize);
            return;
        }

        // IV. reduced% совпадение - мы не можем наверняка оценить неточное совпадение
        if (comparisonScore >= searchVector.Count * MetricsCalculator.ReducedCoefficient)
        {
            DocumentId = documentId;
            Metric = comparisonScore * (1D / reducedTargetVectorSize);
        }
    }

    /// <inheritdoc/>
    public void AppendReduced(int comparisonScore, TokenVector searchVector, DocumentId documentId,
        DirectIndex directIndex)
    {
        // III. 100% совпадение по reduced
        if (comparisonScore == searchVector.Count)
        {
            var reducedTargetVector = directIndex[documentId].Reduced;
            DocumentId = documentId;
            Metric = comparisonScore * (10D / reducedTargetVector.Count);
            return;
        }

        // IV. reduced% совпадение - мы не можем наверняка оценить неточное совпадение
        if (comparisonScore >= searchVector.Count * MetricsCalculator.ReducedCoefficient)
        {
            var reducedTargetVector = directIndex[documentId].Reduced;
            DocumentId = documentId;
            Metric = comparisonScore * (1D / reducedTargetVector.Count);
        }
    }

    /// <inheritdoc/>
    public void Clear()
    {
        ContinueSearching = true;
    }
}
