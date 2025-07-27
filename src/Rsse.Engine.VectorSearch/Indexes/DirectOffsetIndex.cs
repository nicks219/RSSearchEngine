using System.Collections.Generic;
using System.Runtime.InteropServices;
using RsseEngine.Dto;
using RsseEngine.Dto.Offsets;

namespace RsseEngine.Indexes;

/// <summary>
/// Поддержка общего инвертированного индекса "токен-идентификаторы.
/// </summary>
public sealed class DirectOffsetIndex
{
    /// <summary>
    /// Инвертированный индекс: токен в качестве ключа, набор идентификаторов заметок в качестве значения.
    /// </summary>
    private readonly Dictionary<Token, InternalDocumentIdList> _invertedIndex = new();

    private readonly Dictionary<InternalDocumentId, OffsetTokenVector> _directIndex = new();

    private readonly Dictionary<InternalDocumentId, DocumentId> _internalDocumentIdToDocumentId = new();

    private readonly Dictionary<DocumentId, InternalDocumentId> _documentIdToInternalDocumentId = new();

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
        _internalDocumentIdToDocumentId[internalDocumentId] = documentId;

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
            _directIndex.Remove(oldInternalDocumentId);
            _internalDocumentIdToDocumentId.Remove(oldInternalDocumentId);
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
        if (!_invertedIndex.TryGetValue(token, out InternalDocumentIdList documentIds) || documentIds.Count == 0)
        {
            internalDocumentIds = default;
            return false;
        }

        internalDocumentIds = documentIds;
        return true;
    }

    public bool TryGetExternalDocumentId(InternalDocumentId internalDocumentId, out DocumentId externalDocumentId)
    {
        return _internalDocumentIdToDocumentId.TryGetValue(internalDocumentId, out externalDocumentId);
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

    public bool TryGetOffsetTokenVector(InternalDocumentId documentId, out OffsetTokenVector offsetTokenVector)
    {
        return _directIndex.TryGetValue(documentId, out offsetTokenVector);
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

        var offsetTokenVector = new OffsetTokenVector(tokens, offsetInfos, offsets);

        _directIndex.Add(internalDocumentId, offsetTokenVector);
    }
}
