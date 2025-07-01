using RsseEngine.Dto;

namespace RsseEngine.Contracts;

/// <summary>
/// Типизация векторов обратного индекса.
/// </summary>
public interface IDocumentIdCollection
{
    /// <summary>
    /// Длина вектора.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Добавить идентификатор документа в вектор.
    /// </summary>
    /// <param name="docId">Идентификатор документа.</param>
    void Add(DocumentId docId);

    /// <summary>
    /// Удалить идентификатор документа из вектора.
    /// </summary>
    /// <param name="documentId">Идентификатор документа.</param>
    void Remove(DocumentId documentId);

    /// <summary>
    /// Присутствует ли данный идентификатор документа в векторе.
    /// </summary>
    /// <param name="docId">Идентификатор документа.</param>
    /// <returns>Признак присутствия.</returns>
    bool Contains(DocumentId docId);
}
