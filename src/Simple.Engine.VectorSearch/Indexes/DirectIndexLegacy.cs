using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SimpleEngine.Dto.Common;

namespace SimpleEngine.Indexes;

/// <summary>
/// Поддержка общего индекса "идентификатор-токены" для первоначальных алгоритмов.
/// </summary>
public sealed class DirectIndexLegacy
{
    private readonly ConcurrentDictionary<DocumentId, TokenLine> _directIndex = new();

    /// <summary>
    /// Получить количество элементов в индексе.
    /// </summary>
    public int Count => _directIndex.Count;

    /// <summary>
    /// Получить перечислитель для индекса.
    /// </summary>
    /// <returns>Перечислитель.</returns>
    public IEnumerator<KeyValuePair<DocumentId, TokenLine>> GetEnumerator()
    {
        return _directIndex.GetEnumerator();
    }

    /// <summary>
    /// Индексатор по идентификатору.
    /// </summary>
    /// <param name="documentId">Идентификатор документа.</param>
    public TokenLine this[DocumentId documentId] => _directIndex[documentId];

    /// <summary>
    /// Добавить документ в индекс.
    /// </summary>
    /// <param name="documentId">Идентификатор документа.</param>
    /// <param name="tokenLine">Вектора, соответствующие документу.</param>
    /// <returns>Признак успеха операции.</returns>
    public bool TryAdd(DocumentId documentId, TokenLine tokenLine)
    {
        return _directIndex.TryAdd(documentId, tokenLine);
    }

    /// <summary>
    /// Обновить документ в индексе.
    /// </summary>
    /// <param name="documentId">Идентификатор документа.</param>
    /// <param name="tokenLine">Вектора, соответствующие новому документу.</param>
    /// <returns>Признак успеха операции.</returns>
    public bool TryUpdate(DocumentId documentId, TokenLine tokenLine)
    {
        if (!_directIndex.TryGetValue(documentId, out var oldTokenLine))
        {
            return false;
        }

        if (!_directIndex.TryUpdate(documentId, tokenLine, oldTokenLine))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Удалить документ из индекса.
    /// </summary>
    /// <param name="documentId">Идентификатор документа.</param>
    /// <param name="oldTokenLine">Вектора, соответствующие удаленному документу.</param>
    /// <returns>Признак успеха операции.</returns>
    public bool TryRemove(DocumentId documentId, [NotNullWhen(true)] out TokenLine? oldTokenLine)
    {
        return _directIndex.TryRemove(documentId, out oldTokenLine);
    }

    /// <summary>
    /// Очистить индекс.
    /// </summary>
    public void Clear()
    {
        _directIndex.Clear();
    }

    /// <summary>
    /// Получить элемент индекса из требуемой позиции.
    /// </summary>
    /// <param name="index">Позиция элемента.</param>
    /// <returns>Элемент общего индекса.</returns>
    public KeyValuePair<DocumentId, TokenLine> ElementAt(int index)
    {
        return _directIndex.ElementAt(index);
    }

    /// <summary>
    /// Получить первый элемент индекса.
    /// </summary>
    /// <returns>Первый элемент индекса.</returns>
    public KeyValuePair<DocumentId, TokenLine> First()
    {
        return _directIndex.First();
    }

    /// <summary>
    /// Получить последний элемент индекса.
    /// </summary>
    /// <returns>Последний элемент индекса.</returns>
    public KeyValuePair<DocumentId, TokenLine> Last()
    {
        return _directIndex.Last();
    }
}
