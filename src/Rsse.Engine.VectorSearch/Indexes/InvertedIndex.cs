using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using RsseEngine.Dto.Common;
using RsseEngine.Dto.Inverted;

namespace RsseEngine.Indexes;

/// <summary>
/// Поддержка общего инвертированного индекса "токен-идентификаторы.
/// </summary>
public sealed class InvertedIndex(CompactedDictionary.DictionaryStorageType dataPointSearchType)
{
    /// <summary>
    /// Инвертированный индекс: токен в качестве ключа, набор идентификаторов заметок в качестве значения.
    /// </summary>
    private readonly Dictionary<Token, InternalDocumentIds> _invertedIndex = new();

    private readonly List<PositionVectorWrapper> _directIndex = [];

    private readonly List<ExternalDocumentIdWithSize> _externalDocumentIds = [];

    private readonly Dictionary<DocumentId, InternalDocumentId> _actualDocuments = new();

    private readonly List<InternalDocumentId> _deletedDocuments = [];

    /// <summary>
    /// Добавить в индекс вектор токенов и идентификатор соответствующей ему заметки
    /// или обновить индекс для обновляемого документа.
    /// </summary>
    /// <param name="documentId">Идентификатор заметки.</param>
    /// <param name="tokenVector">Вектор токенов.</param>
    /// <returns>Признак добавлен ли документ. Если false - то партиция полностью заполнена.</returns>
    public bool AddOrUpdateVector(DocumentId documentId, TokenVector tokenVector)
    {
        var counter = _externalDocumentIds.Count;

        if (counter > ushort.MaxValue)
        {
            return false;
        }

        var internalDocumentId = new InternalDocumentId(counter);

        RemoveVector(documentId);

        _actualDocuments[documentId] = internalDocumentId;
        _externalDocumentIds.Add(new ExternalDocumentIdWithSize(documentId, tokenVector.Count));

        AppendTokenVector(documentId, internalDocumentId, tokenVector);

        return true;
    }

    /// <summary>
    /// Удалить документ из индекса.
    /// </summary>
    /// <param name="documentId">Идентификатор заметки.</param>
    public void RemoveVector(DocumentId documentId)
    {
        if (_actualDocuments.Remove(documentId, out var oldInternalDocumentId))
        {
            _deletedDocuments.Add(oldInternalDocumentId);
        }
    }

    /// <summary>
    /// Очистить индекс.
    /// </summary>
    public void Clear()
    {
        _invertedIndex.Clear();
        _directIndex.Clear();
        _externalDocumentIds.Clear();
        _actualDocuments.Clear();
        _deletedDocuments.Clear();
    }

    /// <summary>
    /// Получить идентификаторы заметок, в которых присутствует токен.
    /// </summary>
    /// <param name="token">Токен.</param>
    /// <param name="internalDocumentIds">Вектор с идентификаторами заметок.</param>
    /// <returns>Признак наличия токена в индексе.</returns>
    public bool TryGetNonEmptyDocumentIds(Token token, out InternalDocumentIds internalDocumentIds)
    {
        return _invertedIndex.TryGetValue(token, out internalDocumentIds) && internalDocumentIds.Count > 0;
    }

    /// <summary>
    /// Заполнить коллекцию с актуальными идентификаторами, которые соответствуют токенам из запрашиваемого вектора.
    /// </summary>
    /// <param name="tokens">Вектор с целевыми токенами.</param>
    /// <param name="documentIdsCollection">Коллекция токен-идентификаторы.</param>
    public void CreateNonEmptyDocumentIdsCollection(TokenVector tokens, List<InternalDocumentIdsWithToken> documentIdsCollection)
    {
        foreach (var token in tokens)
        {
            if (TryGetNonEmptyDocumentIds(token, out var documentIds))
            {
                documentIdsCollection.Add(new InternalDocumentIdsWithToken(documentIds, token));
            }
        }
    }

    /// <summary>
    /// Заполнить коллекцию актуальными (либо пустыми) идентификаторами, которые соответствуют токенам из запрашиваемого вектора.
    /// </summary>
    /// <param name="tokens">Вектор с целевыми токенами.</param>
    /// <param name="documentIdsCollection">Коллекция токен-идентификаторы.</param>
    public void CreateDocumentIdsCollection(TokenVector tokens, List<InternalDocumentIds> documentIdsCollection)
    {
        var emptyVector = new InternalDocumentIds([]);

        foreach (var token in tokens)
        {
            documentIdsCollection.Add(TryGetNonEmptyDocumentIds(token, out var documentIds)
                ? documentIds
                : emptyVector);
        }
    }

    /// <summary>
    /// Получить документ (если он не помечен к удалению) в виде коллекции токенов и их позиций.
    /// </summary>
    /// <param name="documentId">Идентификатор документа.</param>
    /// <param name="positionVector"></param>
    /// <param name="externalDocument"></param>
    /// <returns></returns>
    public bool TryGetPositionVector(
        InternalDocumentId documentId,
        out PositionVectorWrapper positionVector,
        out ExternalDocumentIdWithSize externalDocument)
    {
        if (_deletedDocuments.Contains(documentId))
        {
            positionVector = default;
            externalDocument = default;
            return false;
        }

        positionVector = _directIndex[documentId.Value];
        // externalDocument = new ExternalDocumentIdWithSize(new DocumentId(positionVector.Value.ExternalId), positionVector.Value.ExternalCount);
        externalDocument = _externalDocumentIds[documentId.Value];
        return true;
    }

    /// <summary>
    /// Добавить заметку в инвертированный индекс.
    /// </summary>
    /// <param name="externalDocumentId">Идентификатор в бд.</param>
    /// <param name="internalDocumentId"></param>
    /// <param name="tokenVector">Заметка в виде вектора.</param>
    private void AppendTokenVector(
        DocumentId externalDocumentId,
        InternalDocumentId internalDocumentId,
        TokenVector tokenVector)
    {
        var positionVectorInternal = tokenVector.ToPositionVector();

        foreach (var (token, _) in positionVectorInternal)
        {
            ref var internalDocumentIds = ref CollectionsMarshal.GetValueRefOrAddDefault(
                _invertedIndex, token, out var exists);

            if (!exists)
            {
                internalDocumentIds = new InternalDocumentIds([]);
            }

            internalDocumentIds.Add(internalDocumentId);
        }

        // токен -> его позиции в тексте
        var positionVectorConverted = positionVectorInternal
            .Select(pair => new KeyValuePair<int, int[]?>(pair.Key.Value, pair.Value.ToArray()))
            .ToDictionary();

        var positionVectorElement = new CompactedDictionary(
            positionVectorConverted,
            externalDocumentId.Value,
            tokenVector.Count,
            dataPointSearchType);

        var positionVector = new PositionVectorWrapper(positionVectorElement);

        _directIndex.Add(positionVector);
    }
}
