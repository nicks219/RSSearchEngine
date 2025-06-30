using RsseEngine.Dto;

namespace RsseEngine.Contracts;

/// <summary>
/// Контракт подсчёта метрик релевантности для результатов поискового запроса.
/// </summary>
public interface IMetricsCalculator
{
    /// <summary>
    /// Добавить метрики для четкого поиска.
    /// </summary>
    /// <param name="comparisonScore">Баллы, полученные поисковым запросом.</param>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <param name="documentId">Идентификатор, полученный при поиске.</param>
    /// <param name="extendedTargetVector">Вектор с заметкой, в которой производился поиск.</param>
    void AppendExtended(int comparisonScore, TokenVector searchVector, DocumentId documentId,
        TokenVector extendedTargetVector);

    /// <summary>
    /// Добавить метрики для нечеткого поиска.
    /// </summary>
    /// <param name="comparisonScore">Баллы, полученные поисковым запросом.</param>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <param name="documentId">Идентификатор, полученный при поиске.</param>
    /// <param name="reducedTargetVector">Вектор с заметкой, в которой производился поиск.</param>
    void AppendReduced(int comparisonScore, TokenVector searchVector, DocumentId documentId,
        TokenVector reducedTargetVector);
}
