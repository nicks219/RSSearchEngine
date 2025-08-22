using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;

namespace RsseEngine.Service;

/// <summary>
/// Подсчёт метрик релевантности для результатов поискового запроса.
/// </summary>
public sealed class MetricsCalculator : IMetricsCalculator
{
    // Коэффициент extended поиска: 0.8D
    internal const double ExtendedCoefficient = 0.8D;

    // Коэффициент reduced поиска: 0.4D
    internal const double ReducedCoefficient = 0.6D; // 0.6 .. 0.75

    /// <inheritdoc/>
    public bool ContinueSearching { get; private set; } = true;

    /// <inheritdoc/>
    public Dictionary<DocumentId, double> ComplianceMetrics { get; } = [];

    /// <inheritdoc/>
    public void AppendExtended(int comparisonScore, TokenVector searchVector, DocumentId documentId,
        int extendedTargetVectorSize)
    {
        // I. 100% совпадение по extended последовательности, по reduced можно не искать
        if (comparisonScore == searchVector.Count)
        {
            ContinueSearching = false;
            ComplianceMetrics.Add(documentId, comparisonScore * (1000D / extendedTargetVectorSize));
            return;
        }

        // II. extended% совпадение
        if (comparisonScore >= searchVector.Count * ExtendedCoefficient)
        {
            // todo: можно так оценить
            // continueSearching = false;
            ComplianceMetrics.Add(documentId, comparisonScore * (100D / extendedTargetVectorSize));
        }
    }

    /// <inheritdoc/>
    public void AppendReduced(int comparisonScore, TokenVector searchVector, DocumentId documentId,
        int reducedTargetVectorSize)
    {
        // III. 100% совпадение по reduced
        if (comparisonScore == searchVector.Count)
        {
            ComplianceMetrics.TryAdd(documentId, comparisonScore * (10D / reducedTargetVectorSize));
            return;
        }

        // IV. reduced% совпадение - мы не можем наверняка оценить неточное совпадение
        if (comparisonScore >= searchVector.Count * ReducedCoefficient)
        {
            ComplianceMetrics.TryAdd(documentId, comparisonScore * (1D / reducedTargetVectorSize));
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
            ComplianceMetrics.TryAdd(documentId, comparisonScore * (10D / reducedTargetVector.Count));
            return;
        }

        // IV. reduced% совпадение - мы не можем наверняка оценить неточное совпадение
        if (comparisonScore >= searchVector.Count * ReducedCoefficient)
        {
            var reducedTargetVector = directIndex[documentId].Reduced;
            ComplianceMetrics.TryAdd(documentId, comparisonScore * (1D / reducedTargetVector.Count));
        }
    }

    /// <inheritdoc/>
    public void Clear()
    {
        ContinueSearching = true;
        ComplianceMetrics.Clear();
    }

    private sealed class NullMetricsCalculator : IMetricsCalculator
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public DocumentId DocumentId { get; private set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public double Metric { get; private set; }

        /// <inheritdoc/>
        public bool ContinueSearching { get; private set; } = true;

        /// <inheritdoc/>
        public Dictionary<DocumentId, double> ComplianceMetrics
        {
            get;
        } = new()
        {
            { new DocumentId(1), 0 }
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
            if (comparisonScore >= searchVector.Count * ExtendedCoefficient)
            {
                DocumentId = documentId;
                // todo: можно так оценить
                // continueSearching = false;
                Metric = comparisonScore * (100D / extendedTargetVectorSize);
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
            if (comparisonScore >= searchVector.Count * ReducedCoefficient)
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
            if (comparisonScore >= searchVector.Count * ReducedCoefficient)
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

    public sealed class Factory(MetricsCalculatorFactoryType factoryType)
    {
        private readonly ConcurrentBag<IMetricsCalculator> _pool = new();

        public IMetricsCalculator CreateMetricsCalculator()
        {
            switch (factoryType)
            {
                case MetricsCalculatorFactoryType.Allocate:
                {
                    return new MetricsCalculator();
                }
                case MetricsCalculatorFactoryType.PoolAllocate:
                {
                    if (!_pool.TryTake(out var metricsCalculator))
                    {
                        metricsCalculator = new MetricsCalculator();
                    }
                    return metricsCalculator;
                }
                case MetricsCalculatorFactoryType.PoolNull:
                {
                    if (!_pool.TryTake(out var metricsCalculator))
                    {
                        metricsCalculator = new NullMetricsCalculator();
                    }
                    return metricsCalculator;
                }
                default:
                {
                    throw new NotSupportedException($"MetricsCalculatorPoolType {factoryType} not supported.");
                }
            }
        }

        public void ReleaseMetricsCalculator(IMetricsCalculator metricsCalculator)
        {
            switch (factoryType)
            {
                case MetricsCalculatorFactoryType.Allocate:
                {
                    break;
                }
                case MetricsCalculatorFactoryType.PoolAllocate:
                case MetricsCalculatorFactoryType.PoolNull:
                {
                    metricsCalculator.Clear();
                    _pool.Add(metricsCalculator);
                    break;
                }
                default:
                {
                    throw new NotSupportedException($"MetricsCalculatorFactoryType {factoryType} not supported.");
                }
            }
        }
    }

    public enum MetricsCalculatorFactoryType
    {
        Allocate,
        PoolAllocate,
        PoolNull
    }
}
