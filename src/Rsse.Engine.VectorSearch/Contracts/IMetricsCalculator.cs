using System.Collections.Generic;
using RsseEngine.Dto;
using RsseEngine.Indexes;

namespace RsseEngine.Contracts;

/// <summary>
/// Контракт подсчёта метрик релевантности для результатов поискового запроса.
/// </summary>
public interface IMetricsCalculator
{
    /// <summary>
    /// Продолжать ли поиск.
    /// </summary>
    bool ContinueSearching { get; }

    /// <summary>
    /// Метрики релевантности.
    /// </summary>
    List<KeyValuePair<DocumentId, double>> ComplianceMetrics { get; }

    /// <summary>
    /// Добавить метрики для четкого поиска.
    /// </summary>
    /// <param name="comparisonScore">Баллы, полученные поисковым запросом.</param>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <param name="documentId">Идентификатор, полученный при поиске.</param>
    /// <param name="extendedTargetVectorSize">Количество токенов в векторе с заметкой, в которой производился поиск.</param>
    void AppendExtended(int comparisonScore, TokenVector searchVector, DocumentId documentId,
        int extendedTargetVectorSize);

    /// <summary>
    /// Добавить метрики для нечеткого поиска.
    /// </summary>
    /// <param name="comparisonScore">Баллы, полученные поисковым запросом.</param>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <param name="documentId">Идентификатор, полученный при поиске.</param>
    /// <param name="reducedTargetVectorSize">Количество токенов в векторе с заметкой, в которой производился поиск.</param>
    void AppendReduced(int comparisonScore, TokenVector searchVector, DocumentId documentId,
        int reducedTargetVectorSize);

    /// <summary>
    /// Добавить метрики для нечеткого поиска.
    /// </summary>
    /// <param name="comparisonScore">Баллы, полученные поисковым запросом.</param>
    /// <param name="searchVector">Вектор с поисковым запросом.</param>
    /// <param name="documentId">Идентификатор, полученный при поиске.</param>
    /// <param name="directIndex">Индекс по идентификаторам заметок.</param>
    void AppendReduced(int comparisonScore, TokenVector searchVector, DocumentId documentId,
        DirectIndex directIndex);

    /// <summary>
    /// Очищает метрики.
    /// </summary>
    void Clear();

    /// <summary>
    /// Количество элементов, которое должно оставаться в метрике.
    /// По умолчанию принимается значение <b>ComplianceSearchService.PageSizeThreshold + 1</b>.
    /// Размер на единичку больше позволит не ломать логику формирования ответа через api.
    /// </summary>
    int Limit { set; }
}
