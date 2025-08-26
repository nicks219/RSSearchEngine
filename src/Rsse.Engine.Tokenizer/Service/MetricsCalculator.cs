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

    /// <summary>
    /// Метрики релевантности для расширенного поиска.
    /// </summary>
    private readonly List<KeyValuePair<DocumentId, double>> _complianceMetricsExtended = [];

    /// <summary>
    /// Метрики релевантности для нечеткого поиска.
    /// </summary>
    private readonly List<KeyValuePair<DocumentId, double>> _complianceMetricsReduced = [];

    /// <summary>
    /// Общая метрика релевантности.
    /// </summary>
    private readonly List<KeyValuePair<DocumentId, double>> _complianceMetrics = [];

    // Минимальный порог для расширенной метрики.
    private double _minThresholdExtended = double.MinValue;

    // Минимальный порог для нечеткой метрики.
    private double _minThresholdReduced = double.MinValue;

    /// <inheritdoc/>
    public bool ContinueSearching { get; private set; } = true;

    /// <inheritdoc/>
    public List<KeyValuePair<DocumentId, double>> ComplianceMetrics
    {
        get
        {
            LimitMetrics();
            return _complianceMetrics;
        }
    }

    /// <inheritdoc/>
    public int Limit { private get; set; } = ComplianceSearchService.PageSizeThreshold + 1;

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

        _complianceMetricsExtended.Clear();
        _complianceMetricsReduced.Clear();
        _complianceMetrics.Clear();

        _minThresholdExtended = double.MinValue;
        _minThresholdReduced = double.MinValue;
    }

    /// <inheritdoc/>
    public void LimitMetrics()
    {
        SortByMetricValue(_complianceMetricsExtended);
        LimitCollection(_complianceMetricsExtended);

        SortByMetricValue(_complianceMetricsReduced);
        LimitCollection(_complianceMetricsReduced);

        MergeMetricCollections(_complianceMetricsExtended, _complianceMetricsReduced);
    }

    /// <summary>
    /// Соединить две коллекции с метриками релевантности, приоритет отдается ключам в первой коллекции.
    /// Результат помещается в ComplianceMetrics.
    /// </summary>
    /// <param name="collectionExtended">Первая коллекция, с расширенными метриками.</param>
    /// <param name="collectionReduced">Вторая коллекция, с нечеткими метриками.</param>
    private void MergeMetricCollections(List<KeyValuePair<DocumentId, double>> collectionExtended,
        List<KeyValuePair<DocumentId, double>> collectionReduced)
    {
        var enumeratorExtended = collectionExtended.GetEnumerator();
        var enumeratorReduced = collectionReduced.GetEnumerator();
        var hasNextExtended = enumeratorExtended.MoveNext();
        var hasNextReduced = enumeratorReduced.MoveNext();

        while (hasNextExtended && hasNextReduced /*&& _complianceMetrics.Count <= Limit*/)
        {
            if (enumeratorExtended.Current.Key != enumeratorReduced.Current.Key)
            {
                break;
            }

            _complianceMetrics.Add(enumeratorExtended.Current);
            hasNextExtended = enumeratorExtended.MoveNext();
            hasNextReduced = enumeratorReduced.MoveNext();
        }

        while (hasNextExtended /*&& _complianceMetrics.Count <= Limit*/)
        {
            _complianceMetrics.Add(enumeratorExtended.Current);
            hasNextExtended = enumeratorExtended.MoveNext();
        }

        while (hasNextReduced /*&& _complianceMetrics.Count <= Limit*/)
        {
            _complianceMetrics.Add(enumeratorReduced.Current);
            hasNextReduced = enumeratorReduced.MoveNext();
        }

        LimitCollection(_complianceMetrics);
    }

    /// <summary>
    /// Добавить расширенную метрику релевантности на документ.
    /// </summary>
    /// <param name="metric">Добавляемая метрика.</param>
    private void AddExtendedMetric(KeyValuePair<DocumentId, double> metric)
    {
        // window должно быть больше Limit, иначе может получиться окно, которое не примет часть элементов
        var windowsSize = Limit * 2;
        if (metric.Value < _minThresholdExtended)
        {
            return;
        }

        _complianceMetricsExtended.Add(metric);
        if (_complianceMetricsExtended.Count < windowsSize)
        {
            return;
        }

        SortByMetricValue(_complianceMetricsExtended);
        LimitCollection(_complianceMetricsExtended);
        _minThresholdExtended = _complianceMetricsExtended.Last().Value;
    }

    /// <summary>
    /// Добавить нечеткую метрику релевантности на документ.
    /// </summary>
    /// <param name="metric">Добавляемая метрика.</param>
    private void AddReducedMetric(KeyValuePair<DocumentId, double> metric)
    {
        // window должно быть больше Limit, иначе может получиться окно, которое не примет часть элементов
        var windowsSize = Limit * 2;
        if (metric.Value < _minThresholdReduced)
        {
            return;
        }

        _complianceMetricsReduced.Add(metric);
        if (_complianceMetricsReduced.Count < windowsSize)
        {
            return;
        }

        SortByMetricValue(_complianceMetricsReduced);
        LimitCollection(_complianceMetricsReduced);
        _minThresholdReduced = _complianceMetricsReduced.Last().Value;
    }

    /// <summary>
    /// Отсортировать коллекцию по значению (метрике) и дополнительно по ключу (идентификатору документа).
    /// </summary>
    /// <param name="collection">Коллекция для сортировки.</param>
    private static void SortByMetricValue(List<KeyValuePair<DocumentId, double>> collection)
    {
        collection
            .Sort((x, y) =>
            {
                var byValueDescending = y.Value.CompareTo(x.Value);
                var thenByKeyDescending = byValueDescending != 0
                    ? byValueDescending
                    : y.Key.CompareTo(x.Key);

                return thenByKeyDescending;
            });
    }

    /// <summary>
    /// Ограничить размер коллекции (до limit элементов).
    /// </summary>
    /// <param name="collection">Ограничиваемая коллекция.</param>
    private void LimitCollection(List<KeyValuePair<DocumentId, double>> collection)
    {
        if (collection.Count > Limit)
        {
            CollectionsMarshal.SetCount(collection, Limit);
        }
    }
}
