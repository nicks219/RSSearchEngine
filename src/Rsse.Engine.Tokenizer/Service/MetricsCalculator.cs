using System;
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

    // Минимальный порог для метрики.
    private double _minThreshold = double.MinValue;

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
            AddMetric(_complianceMetricsExtended, metric);
            return;
        }

        // II. extended% совпадение
        if (comparisonScore >= searchVector.Count * ExtendedCoefficient)
        {
            // todo: можно так оценить
            // continueSearching = false;
            var metric = new KeyValuePair<DocumentId, double>(documentId, comparisonScore * (100D / extendedTargetVectorSize));
            AddMetric(_complianceMetricsExtended, metric);
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
            AddMetric(_complianceMetricsReduced, metric);
            return;
        }

        // IV. reduced% совпадение - мы не можем наверняка оценить неточное совпадение
        if (comparisonScore >= searchVector.Count * ReducedCoefficient)
        {
            var metric = new KeyValuePair<DocumentId, double>(documentId, comparisonScore * (1D / reducedTargetVectorSize));
            AddMetric(_complianceMetricsReduced, metric);
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
            AddMetric(_complianceMetricsReduced, metric);
            return;
        }

        // IV. reduced% совпадение - мы не можем наверняка оценить неточное совпадение
        if (comparisonScore >= searchVector.Count * ReducedCoefficient)
        {
            var reducedTargetVector = directIndex[documentId].Reduced;
            var metric = new KeyValuePair<DocumentId, double>(documentId, comparisonScore * (1D / reducedTargetVector.Count));
            AddMetric(_complianceMetricsReduced, metric);
        }
    }

    /// <inheritdoc/>
    public void Clear()
    {
        ContinueSearching = true;

        _complianceMetricsExtended.Clear();
        _complianceMetricsReduced.Clear();
        _complianceMetrics.Clear();

        _minThreshold = double.MinValue;
    }

    /// <inheritdoc/>
    public void LimitMetrics()
    {
        SortByMetricValue(_complianceMetricsExtended);
        LimitCollection(_complianceMetricsExtended);

        SortByMetricValue(_complianceMetricsReduced);
        LimitCollection(_complianceMetricsReduced);

        MergeMetricCollections();

        SortByMetricValue(_complianceMetrics);
        LimitCollection(_complianceMetrics);
    }

    /// <summary>
    /// Соединить две коллекции с метриками релевантности, приоритет отдается ключам в первой коллекции.
    /// Результат помещается в ComplianceMetrics.
    /// </summary>
    private void MergeMetricCollections()
    {
        if (_complianceMetricsExtended.Count == 0)
        {
            // TODO нужно написать тесты что бы проверялись все условия
            _complianceMetrics.AddRange(_complianceMetricsReduced);
            return;
        }

        if (_complianceMetricsReduced.Count == 0)
        {
            // TODO нужно написать тесты что бы проверялись все условия
            _complianceMetrics.AddRange(_complianceMetricsExtended);
            return;
        }

        _complianceMetrics.Clear();
        _complianceMetricsExtended.Sort((left, right) => left.Key.CompareTo(right.Key));
        _complianceMetricsReduced.Sort((left, right) => left.Key.CompareTo(right.Key));

        var enumeratorExtended = _complianceMetricsExtended.GetEnumerator();
        var enumeratorReduced = _complianceMetricsReduced.GetEnumerator();

        enumeratorExtended.MoveNext();
        enumeratorReduced.MoveNext();

        START:
        if (enumeratorExtended.Current.Key < enumeratorReduced.Current.Key)
        {
            // TODO нужно написать тесты что бы проверялись все условия
            _complianceMetrics.Add(enumeratorExtended.Current);

            if (enumeratorExtended.MoveNext())
            {
                goto START;
            }

            do
            {
                // TODO нужно написать тесты что бы проверялись все условия
                _complianceMetrics.Add(enumeratorReduced.Current);
            } while (enumeratorReduced.MoveNext());
        }
        else if (enumeratorExtended.Current.Key > enumeratorReduced.Current.Key)
        {
            // TODO нужно написать тесты что бы проверялись все условия
            _complianceMetrics.Add(enumeratorReduced.Current);

            if (enumeratorReduced.MoveNext())
            {
                goto START;
            }

            do
            {
                // TODO нужно написать тесты что бы проверялись все условия
                _complianceMetrics.Add(enumeratorExtended.Current);
            } while (enumeratorExtended.MoveNext());
        }
        else
        {
            // TODO нужно написать тесты что бы проверялись все условия
            _complianceMetrics.Add(new KeyValuePair<DocumentId, double>(enumeratorExtended.Current.Key,
                Math.Max(enumeratorExtended.Current.Value, enumeratorReduced.Current.Value)));

            if (enumeratorExtended.MoveNext())
            {
                if (enumeratorReduced.MoveNext())
                {
                    goto START;
                }

                do
                {
                    // TODO нужно написать тесты что бы проверялись все условия
                    _complianceMetrics.Add(enumeratorExtended.Current);
                } while (enumeratorExtended.MoveNext());
            }
            else
            {
                while (enumeratorReduced.MoveNext())
                {
                    // TODO нужно написать тесты что бы проверялись все условия
                    _complianceMetrics.Add(enumeratorReduced.Current);
                }
            }
        }
    }

    /// <summary>
    /// Добавить расширенную метрику релевантности на документ.
    /// </summary>
    /// <param name="complianceMetrics">Коллккция метрик.</param>
    /// <param name="metric">Добавляемая метрика.</param>
    private void AddMetric(List<KeyValuePair<DocumentId, double>> complianceMetrics, KeyValuePair<DocumentId, double> metric)
    {
        // window должно быть больше Limit, иначе может получиться окно, которое не примет часть элементов
        if (metric.Value < _minThreshold)
        {
            return;
        }

        complianceMetrics.Add(metric);

        var windowsSize = Limit * 2;
        if (complianceMetrics.Count < windowsSize)
        {
            return;
        }

        SortByMetricValue(complianceMetrics);
        LimitCollection(complianceMetrics);
        _minThreshold = complianceMetrics.Last().Value;
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
