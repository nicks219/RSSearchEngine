using System;
using RsseEngine.Dto;

namespace RsseEngine.Contracts;

/// <summary>
/// Контракт выбора алгоритма поиска.
/// </summary>
/// <typeparam name="TSearchType">Оптимизация для требуемого алгоритма поиска.</typeparam>
/// <typeparam name="TSearchProcessor">Тип выбираемого алгоритма поиска.</typeparam>
public interface ISearchAlgorithmSelector<in TSearchType, out TSearchProcessor>
    where TSearchType : Enum
{
    /// <summary>
    /// Получить алгоритм поиска с требуемой оптимизацией.
    /// </summary>
    /// <param name="searchType">Оптимизация для требуемого алгоритма поиска.</param>
    /// <returns>Выбираемый алгоритм поиска.</returns>
    TSearchProcessor GetSearchProcessor(TSearchType searchType);

    /// <summary>
    /// Добавить идентификатор документа и его вектор к индексу алгоритма поиска.
    /// </summary>
    /// <param name="documentId">Идентификатор документа.</param>
    /// <param name="tokenVector">Вектор с токенами, представляющими содержание документа.</param>
    void AddVector(DocumentId documentId, TokenVector tokenVector);

    /// <summary>
    /// Актуализировать индекс поиска для обновленного документа.
    /// </summary>
    /// <param name="documentId">Идентификатор документа.</param>
    /// <param name="tokenVector">Новый вектор с токенами, представляющими обновленное содержание документа.</param>
    /// <param name="oldTokenVector">Обновляемый вектор с токенами.</param>
    void UpdateVector(DocumentId documentId, TokenVector tokenVector, TokenVector oldTokenVector);

    /// <summary>
    /// Удалить идентификатор документа и ассоциированный с ним вектор из индекса алгоритма поиска.
    /// </summary>
    /// <param name="documentId">Идентификатор документа.</param>
    /// <param name="tokenVector">Вектор с токенами, представляющими содержание документа.</param>
    void RemoveVector(DocumentId documentId, TokenVector tokenVector);

    /// <summary>
    /// Очистить индекс алгоритма поиска.
    /// </summary>
    void Clear();
}
