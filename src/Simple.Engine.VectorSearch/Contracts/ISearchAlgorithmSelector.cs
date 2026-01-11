using System;
using System.Threading;
using SimpleEngine.Dto.Common;

namespace SimpleEngine.Contracts;

/// <summary>
/// Контракт компонента, предоставляющего доступ к различным алгоритмам поиска.
/// </summary>
/// <typeparam name="TSearchType">Оптимизация для требуемого алгоритма поиска.</typeparam>
/// <typeparam name="TSearchProcessor">Тип требуемого алгоритма поиска.</typeparam>
public interface ISearchAlgorithmSelector<in TSearchType, out TSearchProcessor> where TSearchType : Enum
{
    /// <summary>
    /// Выполнить и посчитать метрики релевантности для поискового запроса.
    /// Добавить в контейнер с результатом.
    /// </summary>
    /// <param name="searchType">Оптимизация для требуемого алгоритма поиска.</param>
    /// <param name="searchVector">Токенизированый текст с поисковым запросом.</param>
    /// <param name="metricsCalculator">Компонент для подсчёта метрик релевантности.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    void Find(TSearchType searchType, TokenVector searchVector,
        IMetricsCalculator metricsCalculator, CancellationToken cancellationToken);

    /// <summary>
    /// Добавить идентификатор документа и его вектор к общему индексу алгоритмов поиска.
    /// </summary>
    /// <param name="documentId">Идентификатор документа.</param>
    /// <param name="tokenVector">Вектор с токенами, представляющими содержание документа.</param>
    void AddVector(DocumentId documentId, TokenVector tokenVector);

    /// <summary>
    /// Актуализировать общий индекс алгоритмов поиска для обновленного документа.
    /// </summary>
    /// <param name="documentId">Идентификатор документа.</param>
    /// <param name="tokenVector">Новый вектор с токенами, представляющими обновленное содержание документа.</param>
    void UpdateVector(DocumentId documentId, TokenVector tokenVector);

    /// <summary>
    /// Удалить идентификатор документа и ассоциированный с ним вектор из общего индекса алгоритмов поиска.
    /// </summary>
    /// <param name="documentId">Идентификатор документа.</param>
    /// <param name="tokenVector">Вектор с токенами, представляющими содержание документа.</param>
    void RemoveVector(DocumentId documentId, TokenVector tokenVector);

    /// <summary>
    /// Очистить общий индекс алгоритмов поиска.
    /// </summary>
    void Clear();
}
