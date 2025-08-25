using System.Collections.Generic;
using Rsse.Domain.Service.Api;
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
    Dictionary<DocumentId, double> ComplianceMetrics { get; }

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
    /// Лимитирование метрик.
    /// </summary>
    /// <param name="count">Количество элементов, которое должно остаться в метрике.</param>
    void Limit(int count = ComplianceSearchService.PageSizeThreshold + 1);
}
