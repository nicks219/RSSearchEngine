using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Rsse.Domain.Service.Api;
using SimpleEngine.Contracts;
using SimpleEngine.Dto.Common;
using SimpleEngine.Indexes;

namespace SimpleEngine.Service;

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
    internal readonly List<KeyValuePair<DocumentId, double>> ComplianceMetricsExtended = [];

    /// <summary>
    /// Метрики релевантности для нечеткого поиска.
    /// </summary>
    internal readonly List<KeyValuePair<DocumentId, double>> ComplianceMetricsReduced = [];

    // Минимальный порог для расширенной метрики.
    private double _minThreshold = double.MinValue;

    /// <inheritdoc/>
    public bool ContinueSearching { get; private set; } = true;

    /// <inheritdoc/>
    public List<KeyValuePair<int, double>> ComplianceMetrics
    {
        get
        {
            var resultComplianceMetrics = new List<KeyValuePair<int, double>>();
            FinalizeMetricsTo(resultComplianceMetrics);
            return resultComplianceMetrics;
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
            AddMetric(ComplianceMetricsExtended, metric);
            return;
        }

        // II. extended% совпадение
        if (comparisonScore >= searchVector.Count * ExtendedCoefficient)
        {
            // todo: можно так оценить
            // continueSearching = false;
            var metric = new KeyValuePair<DocumentId, double>(documentId, comparisonScore * (100D / extendedTargetVectorSize));
            AddMetric(ComplianceMetricsExtended, metric);
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
            AddMetric(ComplianceMetricsReduced, metric);
            return;
        }

        // IV. reduced% совпадение - мы не можем наверняка оценить неточное совпадение
        if (comparisonScore >= searchVector.Count * ReducedCoefficient)
        {
            var metric = new KeyValuePair<DocumentId, double>(documentId, comparisonScore * (1D / reducedTargetVectorSize));
            AddMetric(ComplianceMetricsReduced, metric);
        }
    }

    /// <inheritdoc/>
    public void AppendReduced(int comparisonScore, TokenVector searchVector, DocumentId documentId,
        DirectIndexLegacy directIndexLegacy)
    {
        // III. 100% совпадение по reduced
        if (comparisonScore == searchVector.Count)
        {
            var reducedTargetVector = directIndexLegacy[documentId].Reduced;
            var metric = new KeyValuePair<DocumentId, double>(documentId, comparisonScore * (10D / reducedTargetVector.Count));
            AddMetric(ComplianceMetricsReduced, metric);
            return;
        }

        // IV. reduced% совпадение - мы не можем наверняка оценить неточное совпадение
        if (comparisonScore >= searchVector.Count * ReducedCoefficient)
        {
            var reducedTargetVector = directIndexLegacy[documentId].Reduced;
            var metric = new KeyValuePair<DocumentId, double>(documentId, comparisonScore * (1D / reducedTargetVector.Count));
            AddMetric(ComplianceMetricsReduced, metric);
        }
    }

    /// <inheritdoc/>
    public void Clear()
    {
        ContinueSearching = true;

        ComplianceMetricsExtended.Clear();
        ComplianceMetricsReduced.Clear();

        _minThreshold = double.MinValue;
    }

    /// <summary>
    /// Получить ответ из расширенной и нечеткой метрик.
    /// </summary>
    /// <param name="resultMetrics">Результирующая метрика.</param>
    private void FinalizeMetricsTo(List<KeyValuePair<int, double>> resultMetrics)
    {
        SortByMetricValue(ComplianceMetricsExtended);
        LimitCollection(ComplianceMetricsExtended);

        SortByMetricValue(ComplianceMetricsReduced);
        LimitCollection(ComplianceMetricsReduced);

        MergeMetricCollections(ComplianceMetricsExtended, ComplianceMetricsReduced, resultMetrics);
    }

    /// <summary>
    /// Соединить две коллекции с метриками релевантности, приоритет отдается ключам в первой коллекции.
    /// </summary>
    /// <param name="collectionExtended">Первая коллекция, с расширенными метриками.</param>
    /// <param name="collectionReduced">Вторая коллекция, с нечеткими метриками.</param>
    /// <param name="resultMetrics">Результирующая метрика.</param>
    private void MergeMetricCollections(List<KeyValuePair<DocumentId, double>> collectionExtended,
        List<KeyValuePair<DocumentId, double>> collectionReduced,
        List<KeyValuePair<int, double>> resultMetrics)
    {
        SortByMetricKey(collectionExtended);
        SortByMetricKey(collectionReduced);

        var enumeratorExtended = collectionExtended.GetEnumerator();
        var enumeratorReduced = collectionReduced.GetEnumerator();
        var hasNextExtended = enumeratorExtended.MoveNext();
        var hasNextReduced = enumeratorReduced.MoveNext();

        while (hasNextExtended && hasNextReduced /*&& _complianceMetrics.Count <= Limit*/)
        {
            if (enumeratorExtended.Current.Key == enumeratorReduced.Current.Key)
            {
                resultMetrics.Add(Convert(enumeratorExtended.Current));
                hasNextExtended = enumeratorExtended.MoveNext();
                hasNextReduced = enumeratorReduced.MoveNext();
            }
            else if (enumeratorExtended.Current.Key > enumeratorReduced.Current.Key)
            {
                resultMetrics.Add(Convert(enumeratorExtended.Current));
                hasNextExtended = enumeratorExtended.MoveNext();
            }
            else if (enumeratorExtended.Current.Key < enumeratorReduced.Current.Key)
            {
                resultMetrics.Add(Convert(enumeratorReduced.Current));
                hasNextReduced = enumeratorReduced.MoveNext();
            }
        }

        while (hasNextExtended /*&& _complianceMetrics.Count <= Limit*/)
        {
            resultMetrics.Add(Convert(enumeratorExtended.Current));
            hasNextExtended = enumeratorExtended.MoveNext();
        }

        while (hasNextReduced /*&& _complianceMetrics.Count <= Limit*/)
        {
            resultMetrics.Add(Convert(enumeratorReduced.Current));
            hasNextReduced = enumeratorReduced.MoveNext();
        }

        SortByMetricValue(resultMetrics);
        LimitCollection(resultMetrics);
        return;

        // преобразовать DocumentId в int для KVP
        KeyValuePair<int, double> Convert(KeyValuePair<DocumentId, double> kvp) => new(kvp.Key.Value, kvp.Value);
    }

    /// <summary>
    /// Добавить метрику релевантности на документ к метрикам.
    /// </summary>
    /// <param name="metrics">Расширяемая метрика.</param>
    /// <param name="metric">Добавляемая метрика.</param>
    private void AddMetric(List<KeyValuePair<DocumentId, double>> metrics, KeyValuePair<DocumentId, double> metric)
    {
        // window должно быть больше Limit, иначе может получиться окно, которое не примет часть элементов
        var windowsSize = Limit * 2;
        if (metric.Value < _minThreshold)
        {
            return;
        }

        metrics.Add(metric);
        if (metrics.Count < windowsSize)
        {
            return;
        }

        SortByMetricValue(metrics);
        LimitCollection(metrics);
        _minThreshold = metrics.Last().Value;
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
    /// Отсортировать коллекцию по значению (метрике) и дополнительно по ключу (идентификатору документа).
    /// </summary>
    /// <param name="collection">Коллекция для сортировки.</param>
    private static void SortByMetricValue(List<KeyValuePair<int, double>> collection)
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
    /// Отсортировать коллекцию по ключу (идентификатору документа) и дополнительно по значению (метрике).
    /// </summary>
    /// <param name="collection">Коллекция для сортировки.</param>
    private static void SortByMetricKey(List<KeyValuePair<DocumentId, double>> collection)
    {
        collection
            .Sort((x, y) =>
            {
                var byValueDescending = y.Key.CompareTo(x.Key);
                var thenByKeyDescending = byValueDescending != 0
                    ? byValueDescending
                    : y.Value.CompareTo(x.Value);

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

    /// <summary>
    /// Ограничить размер коллекции (до limit элементов).
    /// </summary>
    /// <param name="collection">Ограничиваемая коллекция.</param>
    private void LimitCollection(List<KeyValuePair<int, double>> collection)
    {
        if (collection.Count > Limit)
        {
            CollectionsMarshal.SetCount(collection, Limit);
        }
    }
}
