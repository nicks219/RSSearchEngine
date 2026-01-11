using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using RD.RsseEngine.Dto;
using RD.RsseEngine.Dto.Offsets;
using RD.RsseEngine.Processor;

namespace RD.RsseEngine.Indexes;

/// <summary>
/// Поддержка общего инвертированного индекса "токен-идентификаторы.
/// </summary>
public sealed class InvertedIndex(DocumentDataPoint.DocumentDataPointSearchType dataPointSearchType)
{
    /// <summary>
    /// Инвертированный индекс: токен в качестве ключа, набор идентификаторов заметок в качестве значения.
    /// </summary>
    private readonly Dictionary<Token, InternalDocumentIdList> _invertedIndex = new();

    private readonly List<ArrayOffsetTokenVector> _directIndex = new();

    private readonly List<ExternalDocumentIdWithSize> _internalDocumentIdToDocumentId = new();

    private readonly Dictionary<DocumentId, InternalDocumentId> _documentIdToInternalDocumentId = new();

    private readonly List<InternalDocumentId> _deletedDocuments = new();

    /// <summary>
    /// Добавить в индекс вектор токенов и идентификатор соответствующей ему заметки
    /// или обновить индекс для обновляемого документа.
    /// </summary>
    /// <param name="documentId">Идентификатор заметки.</param>
    /// <param name="tokenVector">Вектор токенов.</param>
    /// <returns>Признак добавлен ли документ. Если false - то партиция полностью заполнена.</returns>
    public bool AddOrUpdateVector(DocumentId documentId, TokenVector tokenVector)
    {
        var counter = _internalDocumentIdToDocumentId.Count;

        if (counter > ushort.MaxValue)
        {
            return false;
        }

        var internalDocumentId = new InternalDocumentId(counter);

        RemoveVector(documentId);

        _documentIdToInternalDocumentId[documentId] = internalDocumentId;
        _internalDocumentIdToDocumentId.Add(new ExternalDocumentIdWithSize(documentId, tokenVector.Count));

        AppendTokenVector(documentId, internalDocumentId, tokenVector);

        return true;
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
        _deletedDocuments.Clear();
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

    /// <summary>
    /// Заполнить коллекцию векторов с идентификаторами, которые соответствуют токенам из запрашиваемого вектора.
    /// </summary>
    /// <param name="tokens">Вектор с целевыми токенами.</param>
    /// <param name="internalDocumentIds">Коллекция векторов с идентификаторами.</param>
    public void GetNonEmptyDocumentIdVectorsToList(TokenVector tokens,
        List<InternalDocumentIdsWithToken> internalDocumentIds)
    {
        foreach (var token in tokens)
        {
            if (TryGetNonEmptyDocumentIdVector(token, out var documentIds))
            {
                internalDocumentIds.Add(new InternalDocumentIdsWithToken(documentIds, token));
            }
        }
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

    public TfIdfCalculator CreateTfIdfCalculator()
    {
        return new TfIdfCalculator(_invertedIndex, _directIndex);
    }

    public Bm25Calculator CreateBm25Calculator()
    {
        return new Bm25Calculator(_invertedIndex, _directIndex, 1000);
    }
}
