using System.Collections.Concurrent;
using System.Collections.Generic;
using SearchEngine.Tokenizer.Dto;

namespace SearchEngine.Tokenizer;

/// <summary>
/// Подсчёт метрик релевантности для результатов поискового запроса.
/// </summary>
public sealed class MetricsCalculator
{
    // Коэффициент reduced поиска: 0.4D
    internal const double ReducedCoefficient = 0.6D; // 0.6 .. 0.75

    // Коэффициент extended поиска: 0.8D
    private const double ExtendedCoefficient = 0.8D;

    /// <summary>
    /// Продолжать ли поиск.
    /// </summary>
    internal bool ContinueSearching { get; private set; } = true;

    /// <summary>
    /// Метрики релевантности.
    /// </summary>
    internal Dictionary<DocId, double> ComplianceMetrics { get; } = [];

    /// <summary>
    /// Добавить метрики для четкого поиска.
    /// </summary>
    /// <param name="comparisonScore">Баллы, полученные поисковым запросом.</param>
    /// <param name="extendedSearchVector">Вектор с поисковым запросом.</param>
    /// <param name="docId">Идентификатор, полученный при поиске.</param>
    /// <param name="extendedTargetVector">Вектор с заметкой, в которой производился поиск.</param>
    public void AppendExtended(int comparisonScore, TokenVector extendedSearchVector, DocId docId, TokenVector extendedTargetVector)
    {
        // I. 100% совпадение по extended последовательности, по reduced можно не искать
        if (comparisonScore == extendedSearchVector.Count)
        {
            ContinueSearching = false;
            ComplianceMetrics.Add(docId, comparisonScore * (1000D / extendedTargetVector.Count));
            return;
        }

        // II. extended% совпадение
        if (comparisonScore >= extendedSearchVector.Count * ExtendedCoefficient)
        {
            // todo: можно так оценить
            // continueSearching = false;
            ComplianceMetrics.Add(docId, comparisonScore * (100D / extendedTargetVector.Count));
        }
    }

    /// <summary>
    /// Добавить метрики для нечеткого поиска.
    /// </summary>
    /// <param name="comparisonScore">Баллы, полученные поисковым запросом.</param>
    /// <param name="reducedSearchVector">Вектор с поисковым запросом.</param>
    /// <param name="docId">Идентификатор, полученный при поиске.</param>
    /// <param name="reducedTargetVector">Количество токенов в reduced-векторе на котором производился поиск.</param>
    public void AppendReduced(int comparisonScore, TokenVector reducedSearchVector, DocId docId, TokenVector reducedTargetVector)
    {
        // III. 100% совпадение по reduced
        if (comparisonScore == reducedSearchVector.Count)
        {
            ComplianceMetrics.TryAdd(docId, comparisonScore * (10D / reducedTargetVector.Count));
            return;
        }

        // IV. reduced% совпадение - мы не можем наверняка оценить неточное совпадение
        if (comparisonScore >= reducedSearchVector.Count * ReducedCoefficient)
        {
            ComplianceMetrics.TryAdd(docId, comparisonScore * (1D / reducedTargetVector.Count));
        }
    }

    /// <summary>
    /// Добавить метрики для нечеткого поиска.
    /// </summary>
    /// <param name="comparisonScore">Баллы, полученные поисковым запросом.</param>
    /// <param name="reducedSearchVector">Вектор с поисковым запросом.</param>
    /// <param name="docId">Идентификатор, полученный при поиске.</param>
    /// <param name="reducedTargetVectorCount">Длина reduced-вектора заметки, в которой ищем.</param>
    public void AppendReduced(int comparisonScore,
        TokenVector reducedSearchVector,
        DocId docId,
        int reducedTargetVectorCount)
    {
        // III. 100% совпадение по reduced
        if (comparisonScore == reducedSearchVector.Count)
        {
            ComplianceMetrics.TryAdd(docId, comparisonScore * (10D / reducedTargetVectorCount));
            return;
        }

        // IV. reduced% совпадение - мы не можем наверняка оценить неточное совпадение
        if (comparisonScore >= reducedSearchVector.Count * ReducedCoefficient)
        {
            ComplianceMetrics.TryAdd(docId, comparisonScore * (1D / reducedTargetVectorCount));
        }
    }
}
