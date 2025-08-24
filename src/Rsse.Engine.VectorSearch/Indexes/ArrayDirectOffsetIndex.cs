using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using RsseEngine.Dto;
using RsseEngine.Dto.Offsets;

namespace RsseEngine.Indexes;

/// <summary>
/// Поддержка общего инвертированного индекса "токен-идентификаторы.
/// </summary>
public sealed class ArrayDirectOffsetIndex(DocumentDataPoint.DocumentDataPointSearchType dataPointSearchType)
{
    /// <summary>
    /// Инвертированный индекс: токен в качестве ключа, набор идентификаторов заметок в качестве значения.
    /// </summary>
    private readonly Dictionary<Token, InternalDocumentIdList> _invertedIndex = new();

    private readonly List<ArrayOffsetTokenVector> _directIndex = new();

    private readonly List<ExternalDocumentIdWithSize> _internalDocumentIdToDocumentId = new();

    private readonly Dictionary<DocumentId, InternalDocumentId> _documentIdToInternalDocumentId = new();

    private readonly List<InternalDocumentId> _deletedDocuments = new();

    private int _documentIdCounter;

    /// <summary>
    /// Добавить в индекс вектор токенов и идентификатор соответствующей ему заметки
    /// или обновить индекс для обновляемого документа.
    /// </summary>
    /// <param name="documentId">Идентификатор заметки.</param>
    /// <param name="tokenVector">Вектор токенов.</param>
    public void AddOrUpdateVector(DocumentId documentId, TokenVector tokenVector)
    {
        var internalDocumentId = new InternalDocumentId(_documentIdCounter++);

        RemoveVector(documentId);

        _documentIdToInternalDocumentId[documentId] = internalDocumentId;
        _internalDocumentIdToDocumentId.Add(new ExternalDocumentIdWithSize(documentId, tokenVector.Count));

        AppendTokenVector(documentId, internalDocumentId, tokenVector);
    }

    /// <summary>
    /// Удалить документ из индекса.
    /// </summary>
    /// <param name="documentId">Идентификатор заметки.</param>
    public void RemoveVector(DocumentId documentId)
    {
        if (_documentIdToInternalDocumentId.Remove(documentId, out var oldInternalDocumentId))
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
        _internalDocumentIdToDocumentId.Clear();
        _documentIdToInternalDocumentId.Clear();
        _documentIdCounter = 0;
    }

    /// <summary>
    /// Получить идентификаторы заметок, в которых присутствует токен.
    /// </summary>
    /// <param name="token">Токен.</param>
    /// <param name="internalDocumentIds">Вектор с идентификаторами заметок.</param>
    /// <returns>Признак наличия токена в индексе.</returns>
    public bool TryGetNonEmptyDocumentIdVector(Token token, out InternalDocumentIdList internalDocumentIds)
    {
        return _invertedIndex.TryGetValue(token, out internalDocumentIds) && internalDocumentIds.Count > 0;
    }

    public void GetDocumentIdVectorsToList(TokenVector tokens, List<InternalDocumentIdList> internalDocumentIds)
    {
        var emptyDocIdVector = new InternalDocumentIdList(new List<InternalDocumentId>());

        foreach (var token in tokens)
        {
            if (TryGetNonEmptyDocumentIdVector(token, out var documentIds))
            {
                internalDocumentIds.Add(documentIds);
            }
            else
            {
                internalDocumentIds.Add(emptyDocIdVector);
            }
        }
    }

    public bool TryGetOffsetTokenVector(InternalDocumentId documentId,
        out ArrayOffsetTokenVector offsetTokenVector, out ExternalDocumentIdWithSize externalDocument)
    {
        if (_deletedDocuments.Contains(documentId))
        {
            offsetTokenVector = default;
            externalDocument = default;
            return false;
        }

        offsetTokenVector = _directIndex[documentId.Value];
        //externalDocument = new ExternalDocumentIdWithSize(new DocumentId(offsetTokenVector.Value.ExternalId), offsetTokenVector.Value.ExternalCount);
        externalDocument = _internalDocumentIdToDocumentId[documentId.Value];
        return true;
    }

    private void AppendTokenVector(DocumentId externalDocumentId, InternalDocumentId internalDocumentId,
        TokenVector tokenVector)
    {
        var dictionary = tokenVector.ToDictionary();

        foreach (var (token, tokenOffsets) in dictionary)
        {
            ref var internalDocumentIds = ref CollectionsMarshal.GetValueRefOrAddDefault(
                _invertedIndex, token, out var exists);

            if (!exists)
            {
                internalDocumentIds = new InternalDocumentIdList(new List<InternalDocumentId>());
            }

            internalDocumentIds.Add(internalDocumentId);
        }

        var tokens = dictionary.Select(pair => new KeyValuePair<int, int[]?>(pair.Key.Value, pair.Value.ToArray()))
            .ToDictionary();

        var documentDataPoint = new DocumentDataPoint(tokens, externalDocumentId.Value,
            tokenVector.Count, dataPointSearchType);

        var offsetTokenVector = new ArrayOffsetTokenVector(documentDataPoint);

        _directIndex.Add(offsetTokenVector);
    }
}
