using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SimpleEngine.Dto.Common;

namespace SimpleEngine.Indexes;

/// <summary>
/// Поддержка обратного индекса "токен - идентификаторы документов" для наивных алгоритмов.
/// Содержит информацию в каких документах встречаются токены.
/// Методы обновления и добавления оценивают и возвращают успешность попытки, прерываясь при конфликте.
/// Метод добавления успех операции не отслеживает и выполняется до конца.
/// </summary>
public class InvertedIndexLegacy
{
    // размеры для ~1K документов: extended: ~21K | reduced: 10.5K
    // при использовании учитывай уникальность значений в коллекциях идентификаторов
    private readonly ConcurrentDictionary<Token, HashSet<DocumentId>> _invertedIndex = new();

    /// <summary>
    /// Получить количество токенов в индексе.
    /// </summary>
    internal int TokenCount => _invertedIndex.Count;

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
    public bool TryGetIds(Token token, [NotNullWhen(true)] out HashSet<DocumentId>? documentIds) => _invertedIndex.TryGetValue(token, out documentIds);

    /// <summary>
    /// Попытаться добавить все токены из вектора в индекс.
    /// </summary>
    /// <param name="documentId">Идентификатор документа.</param>
    /// <param name="tokenVector">Вектор, соответствующий документу.</param>
    public void TryAddDocument(DocumentId documentId, TokenVector tokenVector)
    {
        foreach (Token token in tokenVector)
        {
            if (!_invertedIndex.TryGetValue(token, out var documentIds))
            {
                _invertedIndex[token] = [documentId];
            }
            else
            {
                documentIds.Add(documentId);
            }
        }
    }

    /// <summary>
    /// Обновить токены из вектора для конкретной заметки в индексе.
    /// </summary>
    /// <param name="documentId">Идентификатор документа.</param>
    /// <param name="newTokenVector">Вектор, соответствующие новому документу.</param>
    /// <param name="oldTokenVector">Вектор, соответствовавшие обновляемому документу.</param>
    /// <returns>Признак успеха операции.</returns>
    public bool TryUpdateDocument(DocumentId documentId, TokenVector newTokenVector, TokenVector oldTokenVector)
    {
        // наивная реализация (удаление/добавление) old: 1 2 | new: 2 3 | common: 2
        // рассмотри вариант с версионированием записей (идентификаторов заметок)

        // todo: методы для множеств аллоцируют хэш-таблицы для источников и используют IEnumerable<T>, оптимизировать
        var common = newTokenVector.GetAsList().Intersect(oldTokenVector.GetAsList()).ToList();
        var forAddition = newTokenVector.GetAsList().Except(common);
        var forDelete = oldTokenVector.GetAsList().Except(common);

        // удаляем:
        foreach (var tokenValue in forDelete)
        {
            var token = new Token(tokenValue);
            if (!_invertedIndex.TryGetValue(token, out var ids))
            {
                // токен отсутствует в индексе
                return false;
            }

            if (!ids.Remove(documentId))
            {
                // документ отсутствует в списке токена
                return false;
            }

            // подумать: у токена может остаться пустая коллекция
            if (ids.Count == 0)
            {
                TryRemoveToken(token, out _);
            }
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
                // документ уже добавлен в список токена, возможна ошибка в логике либо конкурентный доступ
                throw new Exception($"{nameof(TryUpdateDocument)}: failed on addition");
            }
        }

        return true;
    }

    /// <summary>
    /// Удалить идентификатор документ из индекса.
    /// </summary>
    /// <param name="documentId">Идентификатор удаляемого документа.</param>
    /// <param name="oldTokenVector">Вектор, соответствующий удаляемому документу.</param>
    /// <returns>Признак успеха операции.</returns>
    public bool TryRemoveDocument(DocumentId documentId, TokenVector oldTokenVector)
    {
        foreach (var token in oldTokenVector.DistinctAndGet())
        {
            if (!_invertedIndex.TryGetValue(token, out var ids))
            {
                // токен отсутствует в индексе
                return false;
            }

            if (!ids.Remove(documentId))
            {
                // документ отсутствует в списке токена
                return false;
            }
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
