using System.Collections.Generic;
using RsseEngine.Contracts;
using RsseEngine.Dto;

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

    /// <summary>
    /// Продолжать ли поиск.
    /// </summary>
    internal bool ContinueSearching { get; private set; } = true;

    /// <summary>
    /// Метрики релевантности.
    /// </summary>
    internal Dictionary<DocumentId, double> ComplianceMetrics { get; } = [];

    /// <summary>
    /// Добавить метрики для четкого поиска.
    /// </summary>
    /// <param name="comparisonScore">Баллы, полученные поисковым запросом.</param>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <param name="documentId">Идентификатор, полученный при поиске.</param>
    /// <param name="extendedTargetVector">Вектор с заметкой, в которой производился поиск.</param>
    public void AppendExtended(int comparisonScore, TokenVector searchVector, DocumentId documentId, TokenVector extendedTargetVector)
    {
        // I. 100% совпадение по extended последовательности, по reduced можно не искать
        if (comparisonScore == searchVector.Count)
        {
            ContinueSearching = false;
            ComplianceMetrics.Add(documentId, comparisonScore * (1000D / extendedTargetVector.Count));
            return;
        }

        // II. extended% совпадение
        if (comparisonScore >= searchVector.Count * ExtendedCoefficient)
        {
            // todo: можно так оценить
            // continueSearching = false;
            ComplianceMetrics.Add(documentId, comparisonScore * (100D / extendedTargetVector.Count));
        }
    }

    /// <summary>
    /// Добавить метрики для нечеткого поиска.
    /// </summary>
    /// <param name="comparisonScore">Баллы, полученные поисковым запросом.</param>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <param name="documentId">Идентификатор, полученный при поиске.</param>
    /// <param name="reducedTargetVector">Вектор с заметкой, в которой производился поиск.</param>
    public void AppendReduced(int comparisonScore, TokenVector searchVector, DocumentId documentId, TokenVector reducedTargetVector)
    {
        // III. 100% совпадение по reduced
        if (comparisonScore == searchVector.Count)
        {
            ComplianceMetrics.TryAdd(documentId, comparisonScore * (10D / reducedTargetVector.Count));
            return;
        }

        // IV. reduced% совпадение - мы не можем наверняка оценить неточное совпадение
        if (comparisonScore >= searchVector.Count * ReducedCoefficient)
        {
            ComplianceMetrics.TryAdd(documentId, comparisonScore * (1D / reducedTargetVector.Count));
        }
    }
}
