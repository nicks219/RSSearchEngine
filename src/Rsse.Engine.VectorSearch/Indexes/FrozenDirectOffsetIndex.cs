using System.Collections.Frozen;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using RsseEngine.Dto;
using RsseEngine.Dto.Offsets;

namespace RsseEngine.Indexes;

/// <summary>
/// Поддержка общего инвертированного индекса "токен-идентификаторы.
/// </summary>
public sealed class FrozenDirectOffsetIndex : IOffsetIndex
{
    /// <summary>
    /// Инвертированный индекс: токен в качестве ключа, набор идентификаторов заметок в качестве значения.
    /// </summary>
    private readonly Dictionary<Token, InternalDocumentIdList> _invertedIndex = new();

    private readonly List<FrozenOffsetTokenVector> _directIndex = new();

    private readonly List<DocumentId> _internalDocumentIdToDocumentId = new();

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
        _internalDocumentIdToDocumentId.Add(documentId);

        AppendTokenVector(internalDocumentId, tokenVector);
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
            if (TryGetNonEmptyDocumentIdVector(token, out InternalDocumentIdList documentIds))
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
        out FrozenOffsetTokenVector offsetTokenVector, out DocumentId externalDocumentId)
    {
        if (_deletedDocuments.Contains(documentId))
        {
            offsetTokenVector = default;
            externalDocumentId = default;
            return false;
        }

        offsetTokenVector = _directIndex[documentId.Value];
        externalDocumentId = _internalDocumentIdToDocumentId[documentId.Value];
        return true;
    }

    private void AppendTokenVector(InternalDocumentId internalDocumentId, TokenVector tokenVector)
    {
        var dictionary = tokenVector.ToDictionary();

        var tokens = new List<int>();

        foreach (var (token, tokenOffsets) in dictionary)
        {
            ref var internalDocumentIds = ref CollectionsMarshal.GetValueRefOrAddDefault(
                _invertedIndex, token, out var exists);

            if (!exists)
            {
                internalDocumentIds = new InternalDocumentIdList(new List<InternalDocumentId>());
            }

            internalDocumentIds.Add(internalDocumentId);

            tokens.Add(token.Value);
        }

        tokens.Sort();

        var offsetInfos = new List<OffsetInfo>();
        var offsets = new List<int>();

        foreach (var token in tokens)
        {
            var tokenOffsets = dictionary[new Token(token)];

            OffsetInfo.CreateOffsetInfo(tokenOffsets, offsetInfos, offsets);
        }

        Dictionary<int, OffsetInfo> tokens1 = new Dictionary<int, OffsetInfo>();

        for (int i = 0; i < tokens.Count; i++)
        {
            tokens1.Add(tokens[i], offsetInfos[i]);
        }

        FrozenDictionary<int, OffsetInfo> tokens2 = tokens1.ToFrozenDictionary();

        var offsetTokenVector = new FrozenOffsetTokenVector(tokens2, offsets);

        _directIndex.Add(offsetTokenVector);
    }
}
