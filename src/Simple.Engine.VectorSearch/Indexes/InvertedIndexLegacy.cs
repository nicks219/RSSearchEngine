using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SimpleEngine.Dto.Common;

namespace SimpleEngine.Indexes;

/// <summary>
/// Поддержка оОбратного индекса "токен - идентификаторы документов" для первоначальных алгоритмов.
/// </summary>
public class InvertedIndexLegacy
{
    // extended: ~21K | reduced: 10.5K
    private readonly ConcurrentDictionary<Token, HashSet<DocumentId>> _invertedIndex = new();

    /// <summary>
    /// Получить количество элементов в индексе.
    /// </summary>
    public int Count => _invertedIndex.Count;

    /*public /*FrozenDictionary<Token, HashSet<DocumentId>>#1# void Compact()
    {
        var sorted = _invertedIndex
            //.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            .OrderByDescending(x => x.Value.Count)
            .ToList();
        for (var i = 0; i < 10; i++)
        {
            Console.WriteLine(sorted[i].Key + " : " + sorted[i].Value.Count);
        }
        // _invertedIndex.ToFrozenDictionary();
    }*/

    /// <summary>
    /// Получить перечислитель для индекса.
    /// </summary>
    /// <returns>Перечислитель.</returns>
    public IEnumerator<KeyValuePair<Token, HashSet<DocumentId>>> GetEnumerator()
    {
        return _invertedIndex.GetEnumerator();
    }

    /// <summary>
    /// Индексатор по токену.
    /// </summary>
    /// <param name="token">Токен.</param>
    public HashSet<DocumentId> this[Token token] => _invertedIndex[token];

    /// <summary>
    /// Попробовать получить идентификаторы документов по токену.
    /// </summary>
    /// <param name="token">Токен.</param>
    /// <param name="documentIds">Идентификаторы документов.</param>
    /// <returns>Признак успеха.</returns>
    public bool TryGetValue(Token token, [NotNullWhen(true)] out HashSet<DocumentId>? documentIds) => _invertedIndex.TryGetValue(token, out documentIds);

    /// <summary>
    /// Добавить все токены из вектора в индекс.
    /// </summary>
    /// <param name="documentId">Идентификатор документа.</param>
    /// <param name="tokenVector">Вектор, соответствующий документу.</param>
    /// <returns>Признак успеха операции.</returns>
    public bool TryAdd(DocumentId documentId, TokenVector tokenVector)
    {
        // 1. токены могут дублироваться в одном документе, в hashset это будет одно вхождение
        foreach (Token token in tokenVector)
        {
            if (!_invertedIndex.TryGetValue(token, out var documentIds))
            {
                // new HashSet<DocumentId>(32){documentId}
                _invertedIndex[token] = [documentId];
            }
            else
            {
                documentIds.Add(documentId);
            }
        }

        return true;
    }

    /// <summary>
    /// Обновить токены из вектора для конкретной заметки в индексе.
    /// </summary>
    /// <param name="documentId">Идентификатор документа.</param>
    /// <param name="newTokenVector">Вектор, соответствующие новому документу.</param>
    /// <param name="oldTokenVector">Вектор, соответствовавшие обновляемому документу.</param>
    /// <returns>Признак успеха операции.</returns>
    public bool TryUpdate(DocumentId documentId, TokenVector newTokenVector, TokenVector oldTokenVector)
    {
        // наивная реализация (удаление/добавление) old: 1 2 | new: 2 3 | common: 2
        // рассмотри вариант с версионированием записей (идентификаторов заметок)
        var common = newTokenVector.GetAsList().Intersect(oldTokenVector.GetAsList()).ToList();
        var forAddition = newTokenVector.GetAsList().Except(common);
        var forDelete = oldTokenVector.GetAsList().Except(common);
        // удаляем:
        foreach (var tokenValue in forDelete)
        {
            // подумать: у токена может остаться пустая коллекция
            if (_invertedIndex[new Token(tokenValue)].Remove(documentId))
            {
                continue;
            }

            throw new Exception($"{nameof(TryUpdate)}: failed on remove");
            return false;
        }

        // добавляем:
        foreach (var tokenValue in forAddition)
        {
            var token = new Token(tokenValue);
            if (!_invertedIndex.TryGetValue(token, out _))
            {
                // нового токена может не быть в словаре
                _invertedIndex[token] = [documentId];
            }
            else if (!_invertedIndex[token].Add(documentId))
            {
                throw new Exception($"{nameof(TryUpdate)}: failed on addition");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Удалить идентификатор документ из индекса.
    /// </summary>
    /// <param name="documentId">Идентификатор удаляемого документа.</param>
    /// <param name="oldTokenVector">Вектор, соответствовавшие удаляемому документу.</param>
    /// <returns>Признак успеха операции.</returns>
    public bool TryRemoveDocumentId(DocumentId documentId, TokenVector oldTokenVector)
    {
        foreach (var token in oldTokenVector.DistinctAndGet())
        {
            if (_invertedIndex[token].Remove(documentId))
            {
                continue;
            }

            // в значениях нет дубликатов, если в заметке (extended) токены дублировались
            throw new Exception($"{nameof(TryRemoveDocumentId)}: failed on delete document");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Удалить токен и связанные с ним идентификаторы из индекса.
    /// </summary>
    /// <param name="token">Токен.</param>
    /// <param name="oldDocumentIds">Идентификаторы документов, соответствующие удаленному токену.</param>
    /// <returns>Признак успеха операции.</returns>
    public bool TryRemoveToken(Token token, [NotNullWhen(true)] out HashSet<DocumentId>? oldDocumentIds)
    {
        return _invertedIndex.TryRemove(token, out oldDocumentIds);
    }

    /// <summary>
    /// Очистить индекс.
    /// </summary>
    public void Clear()
    {
        _invertedIndex.Clear();
    }
}
