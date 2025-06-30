using System;
using RsseEngine.Dto;

namespace RsseEngine.Contracts;

/// <summary>
/// Контракт компонента, предоставляющего доступ к различным алгоритмам поиска.
/// </summary>
/// <typeparam name="TSearchType">Оптимизация для требуемого алгоритма поиска.</typeparam>
/// <typeparam name="TSearchProcessor">Тип требуемого алгоритма поиска.</typeparam>
public interface ISearchAlgorithmSelector<in TSearchType, out TSearchProcessor>
    where TSearchType : Enum
{
    /// <summary>
    /// Получить необходимый функционал поиска с требуемой оптимизацией.
    /// </summary>
    /// <param name="searchType">Оптимизация для требуемого алгоритма поиска.</param>
    /// <returns>Функционал поиска с запрошенным алгоритмом.</returns>
    TSearchProcessor GetSearchProcessor(TSearchType searchType);

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
    /// <param name="oldTokenVector">Обновляемый вектор с токенами.</param>
    void UpdateVector(DocumentId documentId, TokenVector tokenVector, TokenVector oldTokenVector);

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
