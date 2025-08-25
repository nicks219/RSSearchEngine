using System.Collections.Generic;
using System.Linq;
using Rsse.Domain.Service.Api;
using RsseEngine.Contracts;
using RsseEngine.Dto;
using RsseEngine.Indexes;

namespace RsseEngine.Service;

/// <summary>
/// Функционал подсчёта метрик релевантности для результатов поискового запроса.
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
    public List<KeyValuePair<DocumentId, double>> ComplianceMetrics { get; private set; } = [];

    /// <inheritdoc/>
    public void AppendExtended(int comparisonScore, TokenVector searchVector, DocumentId documentId,
        int extendedTargetVectorSize)
    {
        // I. 100% совпадение по extended последовательности, по reduced можно не искать
        if (comparisonScore == searchVector.Count)
        {
            ContinueSearching = false;
            var metric = new KeyValuePair<DocumentId, double>(documentId, comparisonScore * (1000D / extendedTargetVectorSize));
            AddMetric(metric);
            return;
        }

        // II. extended% совпадение
        if (comparisonScore >= searchVector.Count * ExtendedCoefficient)
        {
            // todo: можно так оценить
            // continueSearching = false;
            var metric = new KeyValuePair<DocumentId, double>(documentId, comparisonScore * (100D / extendedTargetVectorSize));
            AddMetric(metric);
        }
    }

    /// <inheritdoc/>
    public void AppendReduced(int comparisonScore, TokenVector searchVector, DocumentId documentId,
        int reducedTargetVectorSize)
    {
        // III. 100% совпадение по reduced
        if (comparisonScore == searchVector.Count)
        {
            var metric = new KeyValuePair<DocumentId, double>(documentId, comparisonScore * (10D / reducedTargetVectorSize));
            TryAddMetric(metric);
            return;
        }

        // IV. reduced% совпадение - мы не можем наверняка оценить неточное совпадение
        if (comparisonScore >= searchVector.Count * ReducedCoefficient)
        {
            var metric = new KeyValuePair<DocumentId, double>(documentId, comparisonScore * (1D / reducedTargetVectorSize));
            TryAddMetric(metric);
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
            var metric = new KeyValuePair<DocumentId, double>(documentId, comparisonScore * (10D / reducedTargetVector.Count));
            TryAddMetric(metric);
            return;
        }

        // IV. reduced% совпадение - мы не можем наверняка оценить неточное совпадение
        if (comparisonScore >= searchVector.Count * ReducedCoefficient)
        {
            var reducedTargetVector = directIndex[documentId].Reduced;
            var metric = new KeyValuePair<DocumentId, double>(documentId, comparisonScore * (1D / reducedTargetVector.Count));
            TryAddMetric(metric);
        }
    }

    /// <inheritdoc/>
    public void Clear()
    {
        ContinueSearching = true;
        ComplianceMetrics.Clear();
    }

    /// <inheritdoc/>
    public void Limit(int limit)
    {
        // const int maxTestMetricCount = 147;
        ComplianceMetrics = ComplianceMetrics
            .OrderByDescending(kvp => kvp.Value)
            .ThenByDescending(kvp => kvp.Key)
            .Take(limit)
            .ToList();
    }

    /// <summary>
    /// Добавить метрику релевантности на документ.
    /// </summary>
    /// <param name="metric">Добавляемая метрика.</param>
    private void AddMetric(KeyValuePair<DocumentId, double> metric)
    {
        ComplianceMetrics.Add(metric);
        TryResize();
    }

    /// <summary>
    /// Добавить метрику релевантности на документ только если метрика на данный документ не была добавлена ранее.
    /// </summary>
    /// <param name="metric">Добавляемая метрика.</param>
    private void TryAddMetric(KeyValuePair<DocumentId, double> metric)
    {
        if (ComplianceMetrics.All(kvp => kvp.Key != metric.Key))
        {
            ComplianceMetrics.Add(metric);
            TryResize();
        }
    }

    /// <summary>
    /// Ограничить количество элементов в метрике при достижении определенного порога.
    /// </summary>
    private void TryResize()
    {
        const int windowsSize = ComplianceSearchService.PageSizeThreshold * 2;
        var currentCount = ComplianceMetrics.Count;

        if (currentCount < windowsSize)
        {
            return;
        }

        // +1 чтобы не ломать логику ответа через api
        Limit(ComplianceSearchService.PageSizeThreshold + 1);
    }
}
