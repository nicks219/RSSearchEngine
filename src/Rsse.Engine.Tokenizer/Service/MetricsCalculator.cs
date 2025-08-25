using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
    // 0. заведи два листа - или три?
    // 1. финальный LimitMetrics вызывать в get, там же смержить два списка
    public List<KeyValuePair<DocumentId, double>> ComplianceMetrics { get; private set; } = [];

    /// <summary>
    /// Метрики релевантности для расширенного поиска.
    /// </summary>
    private List<KeyValuePair<DocumentId, double>> ComplianceMetricsExtended { get; set;} = [];

    /// <summary>
    /// Метрики релевантности для нечеткого поиска.
    /// </summary>
    private List<KeyValuePair<DocumentId, double>> ComplianceMetricsReduced { get; set;} = [];

    /// <inheritdoc/>
    public void AppendExtended(int comparisonScore, TokenVector searchVector, DocumentId documentId,
        int extendedTargetVectorSize)
    {
        // I. 100% совпадение по extended последовательности, по reduced можно не искать
        if (comparisonScore == searchVector.Count)
        {
            ContinueSearching = false;
            var metric = new KeyValuePair<DocumentId, double>(documentId, comparisonScore * (1000D / extendedTargetVectorSize));
            AddExtendedMetric(metric);
            return;
        }

        // II. extended% совпадение
        if (comparisonScore >= searchVector.Count * ExtendedCoefficient)
        {
            // todo: можно так оценить
            // continueSearching = false;
            var metric = new KeyValuePair<DocumentId, double>(documentId, comparisonScore * (100D / extendedTargetVectorSize));
            AddExtendedMetric(metric);
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
            AddReducedMetric(metric);
            return;
        }

        // IV. reduced% совпадение - мы не можем наверняка оценить неточное совпадение
        if (comparisonScore >= searchVector.Count * ReducedCoefficient)
        {
            var metric = new KeyValuePair<DocumentId, double>(documentId, comparisonScore * (1D / reducedTargetVectorSize));
            AddReducedMetric(metric);
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
            AddReducedMetric(metric);
            return;
        }

        // IV. reduced% совпадение - мы не можем наверняка оценить неточное совпадение
        if (comparisonScore >= searchVector.Count * ReducedCoefficient)
        {
            var reducedTargetVector = directIndex[documentId].Reduced;
            var metric = new KeyValuePair<DocumentId, double>(documentId, comparisonScore * (1D / reducedTargetVector.Count));
            AddReducedMetric(metric);
        }
    }

    /// <inheritdoc/>
    public void Clear()
    {
        ContinueSearching = true;
        ComplianceMetrics.Clear();


        ComplianceMetricsExtended.Clear();
        ComplianceMetricsReduced.Clear();
    }

    /// <inheritdoc/>
    public void LimitMetrics()
    {
        // 2. использовать Sort на листе
        /*ComplianceMetrics = ComplianceMetrics
            .OrderByDescending(kvp => kvp.Value)
            .ThenByDescending(kvp => kvp.Key)
            .Take(Limit)
            .ToList();*/

        /*ComplianceMetrics
            .Sort((x, y) =>
            {
                var byValueDescending = y.Value.CompareTo(x.Value);
                var thenByKeyDescending = byValueDescending != 0
                    ? byValueDescending
                    : y.Key.CompareTo(x.Key);

                return thenByKeyDescending;
            });
        ComplianceMetrics = ComplianceMetrics.Take(Limit).ToList();*/

        // сливаем две метрики в одну
        ComplianceMetricsExtended
            .Sort((x, y) =>
            {
                var byValueDescending = y.Value.CompareTo(x.Value);
                var thenByKeyDescending = byValueDescending != 0
                    ? byValueDescending
                    : y.Key.CompareTo(x.Key);

                return thenByKeyDescending;
            });
        if (ComplianceMetricsExtended.Count > Limit)
        {
            CollectionsMarshal.SetCount(ComplianceMetricsExtended, Limit);
            //ComplianceMetricsExtended = ComplianceMetricsExtended.Take(Limit).ToList();
        }

        ComplianceMetricsReduced
            .Sort((x, y) =>
            {
                var byValueDescending = y.Value.CompareTo(x.Value);
                var thenByKeyDescending = byValueDescending != 0
                    ? byValueDescending
                    : y.Key.CompareTo(x.Key);

                return thenByKeyDescending;
            });
        if (ComplianceMetricsReduced.Count > Limit)
        {
            CollectionsMarshal.SetCount(ComplianceMetricsReduced, Limit);
            //ComplianceMetricsReduced = ComplianceMetricsReduced.Take(Limit).ToList();
        }

        var asEnumerable = ComplianceMetricsExtended
            .Concat(
                ComplianceMetricsReduced.Where(x =>
                    ComplianceMetricsExtended.All(f => f.Key != x.Key)));

        // вариант ComplianceMetrics = .Take(Limit).ToList();
        foreach (var kvp in asEnumerable)
        {
            ComplianceMetrics.Add(kvp);
            //if (ComplianceMetrics.Count == Limit) break;
        }
    }

    /// <inheritdoc/>
    public int Limit { private get; set; } = ComplianceSearchService.PageSizeThreshold + 1;

    /// <summary>
    /// Добавить метрику релевантности на документ.
    /// </summary>
    /// <param name="metric">Добавляемая метрика.</param>
    private void AddExtendedMetric(KeyValuePair<DocumentId, double> metric)
    {
        ComplianceMetricsExtended.Add(metric);

        //ComplianceMetrics.Add(metric);
        //TryResize();
    }

    /// <summary>
    /// Добавить метрику релевантности на документ, только если метрика на данный документ не была добавлена ранее.
    /// </summary>
    /// <param name="metric">Добавляемая метрика.</param>
    private void AddReducedMetric(KeyValuePair<DocumentId, double> metric)
    {
        ComplianceMetricsReduced.Add(metric);

        //if (ComplianceMetrics.All(kvp => kvp.Key != metric.Key))
        //{
            //ComplianceMetrics.Add(metric);
            //TryResize();
        //}
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

        LimitMetricsInternal();
        // 3. после первой обрезки появится минимальный порог, ниже которого не надо вставлять
    }

    private void LimitMetricsInternal()
    {
        // 2. использовать Sort на листе
        /*ComplianceMetrics = ComplianceMetrics
            .OrderByDescending(kvp => kvp.Value)
            .ThenByDescending(kvp => kvp.Key)
            .Take(Limit)
            .ToList();*/

        /*ComplianceMetrics
            .Sort((x, y) =>
            {
                var byValueDescending = y.Value.CompareTo(x.Value);
                var thenByKeyDescending = byValueDescending != 0
                    ? byValueDescending
                    : y.Key.CompareTo(x.Key);

                return thenByKeyDescending;
            });

        ComplianceMetrics = ComplianceMetrics.Take(Limit).ToList();*/
    }
}
