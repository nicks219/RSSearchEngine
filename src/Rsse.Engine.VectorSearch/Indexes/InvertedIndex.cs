using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RsseEngine.Contracts;
using RsseEngine.Dto;

namespace RsseEngine.Indexes;

/// <summary>
/// Поддержка общего инвертированного индекса "токен-идентификаторы.
/// </summary>
public sealed class InvertedIndex<TDocumentIdCollection>
    where TDocumentIdCollection : struct, IDocumentIdCollection
{
    /// <summary>
    /// Инвертированный индекс: токен в качестве ключа, набор идентификаторов заметок в качестве значения.
    /// </summary>
    private readonly Dictionary<Token, TDocumentIdCollection> _invertedIndex = new();

    /// <summary>
    /// Получить коллекцию векторов с идентификаторами, которые соответствуют токенам из запрашиваемого вектора.
    /// </summary>
    /// <param name="tokenVector">Вектор с целевыми токенами.</param>
    /// <returns>Коллекция векторов с идентификаторами.</returns>
    public List<TDocumentIdCollection> Get(TokenVector tokenVector)
    {
        List<TDocumentIdCollection> result = new();

        foreach (var token in tokenVector)
        {
            if (_invertedIndex.TryGetValue(token, out TDocumentIdCollection documentIdVector))
            {
                result.Add(documentIdVector);
            }
        }

        return result;
    }

    /// <summary>
    /// Добавить в индекс вектор токенов и идентификатор соответствующей ему заметки.
    /// </summary>
    /// <param name="documentId">Идентификатор заметки.</param>
    /// <param name="tokenVector">Вектор токенов.</param>
    public void AddVector(DocumentId documentId, TokenVector tokenVector)
    {
        foreach (Token token in tokenVector)
        {
            ref TDocumentIdCollection collection = ref CollectionsMarshal.GetValueRefOrAddDefault(
                _invertedIndex, token, out bool exists);

            if (!exists)
            {
                collection = CreateCollection();
            }

            collection.Add(documentId);
        }
    }

    /// <summary>
    /// Обновить индекс для обновляемого документа.
    /// </summary>
    /// <param name="documentId">Идентификатор заметки.</param>
    /// <param name="tokenVector">Новый вектор токенов, соответсвующий обновленной заметке.</param>
    /// <param name="oldTokenVector">Старый вектор токенов, соответсвующий обновляемой заметке.</param>
    public void UpdateVector(DocumentId documentId, TokenVector tokenVector, TokenVector oldTokenVector)
    {
        tokenVector = tokenVector.DistinctAndGet();
        oldTokenVector = oldTokenVector.DistinctAndGet();

        var intersection = tokenVector.Intersect(oldTokenVector);

        foreach (Token token in oldTokenVector)
        {
            if (!intersection.Contains(token))
            {
                _invertedIndex[token].Remove(documentId);
            }
        }

        foreach (Token token in tokenVector)
        {
            if (!intersection.Contains(token))
            {
                ref TDocumentIdCollection collection = ref CollectionsMarshal.GetValueRefOrAddDefault(
                    _invertedIndex, token, out bool exists);

                if (!exists)
                {
                    collection = CreateCollection();
                }

                collection.Add(documentId);
            }
        }
    }

    /// <summary>
    /// Удалить документ из индекса.
    /// </summary>
    /// <param name="documentId">Идентификатор заметки.</param>
    /// <param name="tokenVector">Вектор, соответсвующий удаляемой заметке.</param>
    public void RemoveVector(DocumentId documentId, TokenVector tokenVector)
    {
        tokenVector = tokenVector.DistinctAndGet();

        foreach (var token in tokenVector)
        {
            _invertedIndex[token].Remove(documentId);
        }
    }

    /// <summary>
    /// Очистить индекс.
    /// </summary>
    public void Clear()
    {
        _invertedIndex.Clear();
    }

    /// <summary>
    /// Получить идентификаторы заметок, в которых присутствует токен.
    /// </summary>
    /// <param name="token">Токен.</param>
    /// <param name="documentIdVector">Вектор с идентификаторами заметок.</param>
    /// <returns>Признак наличия токена в индексе.</returns>
    public bool TryGetNonEmptyDocumentIdVector(Token token, out TDocumentIdCollection documentIdVector)
    {
        return _invertedIndex.TryGetValue(token, out documentIdVector)
            && documentIdVector.Count > 0;
    }

    /// <summary>
    /// Определить, присутствует ли любой токен из вектора в заданной заметке.
    /// </summary>
    /// <param name="vector">Вектор токенов.</param>
    /// <param name="documentId">Идентификатор заметки, для которой мы проверяем наличие любого токена из вектора.</param>
    /// <returns><b>true</b> - Один из токенов присутствует в заданной заметке.</returns>
    public bool ContainsAnyTokenForDoc(TokenVector vector, DocumentId documentId)
    {
        foreach (var token in vector)
        {
            if (ContainsDocumentIdForToken(token, documentId))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Определить, присутствует ли идентификатор заметки для токена.
    /// </summary>
    /// <param name="token">Токен.</param>
    /// <param name="documentId">Идентификатор заметки.</param>
    /// <returns><b>true</b> - Идентификатор заметки присутствует для токена.</returns>
    public bool ContainsDocumentIdForToken(Token token, DocumentId documentId)
    {
        return TryGetNonEmptyDocumentIdVector(token, out var reducedTokens)
               && reducedTokens.Contains(documentId);
    }

    /// <summary>
    /// Создать экземпляр коллекции требуемого типа.
    /// </summary>
    /// <returns>Экземпляр коллекции типа <b>TDocumentIdCollection</b>.</returns>
    /// <exception cref="NotSupportedException">Требуемый тип не поддерживается.</exception>
    private static TDocumentIdCollection CreateCollection()
    {
        if (typeof(TDocumentIdCollection) == typeof(DocumentIdSet))
        {
            var documentIdSet = new DocumentIdSet([]);
            return Unsafe.As<DocumentIdSet, TDocumentIdCollection>(ref documentIdSet);
        }

        if (typeof(TDocumentIdCollection) == typeof(DocumentIdList))
        {
            var documentIdList = new DocumentIdList([]);
            return Unsafe.As<DocumentIdList, TDocumentIdCollection>(ref documentIdList);
        }

        throw new NotSupportedException();
    }
}
