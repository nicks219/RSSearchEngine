using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    private readonly Dictionary<Token, TDocumentIdCollection> _documentIdCollections = new();

    /// <summary>
    /// Получить коллекцию векторов с идентификаторами, которые соответствуют токенам из запрашиваемого вектора.
    /// </summary>
    /// <param name="tokenVector">Вектор с целевыми токенами.</param>
    /// <returns>Коллекция векторов с идентификаторами.</returns>
    public List<TDocumentIdCollection> Get(TokenVector tokenVector)
    {
        List<TDocumentIdCollection> result = new();

        foreach (Token token in tokenVector)
        {
            if (_documentIdCollections.TryGetValue(token, out TDocumentIdCollection documentIdVector))
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
                _documentIdCollections, token, out bool exists);

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

        HashSet<Token> intersection = tokenVector.Intersect(oldTokenVector);

        foreach (Token token in oldTokenVector)
        {
            if (!intersection.Contains(token))
            {
                _documentIdCollections[token].Remove(documentId);
            }
        }

        foreach (Token token in tokenVector)
        {
            if (!intersection.Contains(token))
            {
                ref TDocumentIdCollection collection = ref CollectionsMarshal.GetValueRefOrAddDefault(
                    _documentIdCollections, token, out bool exists);

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

        foreach (Token token in tokenVector)
        {
            _documentIdCollections[token].Remove(documentId);
        }
    }

    /// <summary>
    /// Очистить индекс.
    /// </summary>
    public void Clear()
    {
        _documentIdCollections.Clear();
    }

    /// <summary>
    /// Получить идентификаторы заметок, в которых присутствует токен.
    /// </summary>
    /// <param name="token">Токен.</param>
    /// <param name="documentIdSet">Вектор с идентификаторами заметок.</param>
    /// <returns>Признак наличия токена в индексе.</returns>
    public bool TryGetIdentifiers(Token token, [MaybeNullWhen(false)] out TDocumentIdCollection documentIdSet) =>
        _documentIdCollections.TryGetValue(token, out documentIdSet);

    /// <summary>
    /// Определить, присутствует ли любой токен из вектора в заданной заметке.
    /// </summary>
    /// <param name="vector">Вектор токенов.</param>
    /// <param name="id">Идентификатор заметки, для которой мы проверяем наличие любого токена из вектора.</param>
    /// <returns><b>true</b> - Один из токенов присутствует в заданной заметке.</returns>
    public bool ContainsAnyTokenForDoc(TokenVector vector, DocumentId id)
    {
        foreach (var token in vector)
        {
            if (!_documentIdCollections.TryGetValue(token, out var docIdVector))
            {
                continue;
            }

            if (docIdVector.Contains(id))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Определить, присутствует ли токен в любой из заданных заметок.
    /// </summary>
    /// <param name="token">Токен.</param>
    /// <param name="documentIdSet">Идентификаторы заметок, в которых мы проверяем наличие токена.</param>
    /// <returns><b>true</b> - Токен присутствует в одной из заданных заметок.</returns>
    // варианты нейминга: ContainsInAny | ContainsTokenInAnyGivenIds
    [Obsolete("в данный момент не используется")]
    public bool ContainsAnyDocForToken(Token token, DocumentIdSet documentIdSet)
    {
        foreach (var id in documentIdSet)
        {
            if (!_documentIdCollections.TryGetValue(token, out var ginIds))
            {
                continue;
            }

            if (ginIds.Contains(id))
            {
                return true;
            }
        }

        return false;
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
            DocumentIdSet documentIdList = new DocumentIdSet(new HashSet<DocumentId>());
            return Unsafe.As<DocumentIdSet, TDocumentIdCollection>(ref documentIdList);
        }

        if (typeof(TDocumentIdCollection) == typeof(DocumentIdList))
        {
            DocumentIdList documentIdList = new DocumentIdList(new List<DocumentId>());
            return Unsafe.As<DocumentIdList, TDocumentIdCollection>(ref documentIdList);
        }

        throw new NotSupportedException();
    }
}
