using System;
using System.Collections.Generic;
using System.Linq;
using Rsse.Domain.Service.Api;
using Rsse.Domain.Service.Configuration;
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
    public void Limit(int count)
    {
        var isTesting = Environment.GetEnvironmentVariable(Constants.AspNetCoreEnvironmentName) == Constants.TestingEnvironment;
        if (isTesting)
        {
            // максимальное количество метрик в тестовом ответе
            // todo: как лучше сконфигурировать лимит для различных тестов?
            count = 147;
        }

        ComplianceMetrics = ComplianceMetrics
        // сортировка, применяемая в тесте для стабилизации результата выдачи
        // .OrderByDescending(x => x.Value)
        // .ThenByDescending(x => x.Key)
        .Take(count)
        .ToList();
    }

    /// <summary>
    /// Добавить метрику релевантности на документ.
    /// </summary>
    /// <param name="metric">Добавляемая метрика.</param>
    private void AddMetric(KeyValuePair<DocumentId, double> metric)
    {
        AddMetricWithWindow(metric);
    }

    /// <summary>
    /// Добавить метрику релевантности на документ только если метрика на данный документ не была добавлена ранее.
    /// </summary>
    /// <param name="metric">Добавляемая метрика.</param>
    private void TryAddMetric(KeyValuePair<DocumentId, double> metric)
    {
        if (ComplianceMetrics.All(kvp => kvp.Key != metric.Key))
        {
            AddMetricWithWindow(metric);
        }
    }

    /// <summary>
    /// Добавить метрику, сохраняя константное значение элементов в списке.
    /// </summary>
    /// <param name="metric">Добавляемая метрика.</param>
    private void AddMetricWithWindow(KeyValuePair<DocumentId, double> metric)
    {
        ComplianceMetrics.Add(metric);
        // ограничиваемся 11 значениями: если при получении ответа его размер не дойдёт до ресайза, то в нём будет больше 11 элементов
        // todo: при получении метрики в качестве ответа необходимо еще раз вызывать её ограничение по размеру
        // например в SearchEngineManager.FindExtended / FindReduced
        const int maxCount = ComplianceSearchService.PageSizeThreshold + 1;
        var count = ComplianceMetrics.Count;

        if (count < maxCount)
        {
            return;
        }

        // изменяем размер списка
        const int maxTestMetricCount = 147;
        ComplianceMetrics = ComplianceMetrics
            .OrderByDescending(kvp => kvp.Value)
            .ThenByDescending(kvp => kvp.Key)
            .Take(maxTestMetricCount)
            .ToList();
    }
}
