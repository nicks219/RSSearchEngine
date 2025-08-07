using System.Collections.Generic;
using System.Runtime.InteropServices;
using RsseEngine.Dto;
using RsseEngine.Dto.Offsets;

namespace RsseEngine.Indexes;

/// <summary>
/// Поддержка общего инвертированного индекса "токен-идентификаторы.
/// </summary>
public sealed class InvertedOffsetIndex
{
    /// <summary>
    /// Инвертированный индекс: токен в качестве ключа, набор идентификаторов заметок в качестве значения.
    /// </summary>
    private readonly Dictionary<Token, DocumentIdsWithOffsets> _invertedIndex = new();

    private readonly Dictionary<InternalDocumentId, ExternalDocumentIdWithSize> _internalDocumentIdToDocumentId = new();

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
        _internalDocumentIdToDocumentId[internalDocumentId] = new ExternalDocumentIdWithSize(documentId, tokenVector.Count);

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
            _internalDocumentIdToDocumentId.Remove(oldInternalDocumentId);
        }
    }

    /// <summary>
    /// Очистить индекс.
    /// </summary>
    public void Clear()
    {
        _invertedIndex.Clear();
        _internalDocumentIdToDocumentId.Clear();
        _documentIdToInternalDocumentId.Clear();
        _documentIdCounter = 0;
    }

    /// <summary>
    /// Получить идентификаторы заметок, в которых присутствует токен.
    /// </summary>
    /// <param name="token">Токен.</param>
    /// <param name="documentDocumentIdsVector">Вектор с идентификаторами заметок.</param>
    /// <returns>Признак наличия токена в индексе.</returns>
    public bool TryGetNonEmptyDocumentIdVector(Token token, out DocumentIdsWithOffsets documentDocumentIdsVector)
    {
        if (!_invertedIndex.TryGetValue(token, out DocumentIdsWithOffsets documentIds) || documentIds.DocumentIds.Count == 0)
        {
            documentDocumentIdsVector = default;
            return false;
        }

        documentDocumentIdsVector = documentIds;
        return true;
    }

    public bool TryGetExternalDocumentId(InternalDocumentId internalDocumentId, out ExternalDocumentIdWithSize externalDocument)
    {
        return _internalDocumentIdToDocumentId.TryGetValue(internalDocumentId, out externalDocument);
    }

    private void AppendTokenVector(InternalDocumentId internalDocumentId, TokenVector tokenVector)
    {
        var dictionary = tokenVector.ToDictionary();

        foreach (var (token, tokenOffsets) in dictionary)
        {
            ref var documentIdsWithOffsets = ref CollectionsMarshal.GetValueRefOrAddDefault(
                _invertedIndex, token, out var exists);

            if (!exists)
            {
                documentIdsWithOffsets = new DocumentIdsWithOffsets(
                    new InternalDocumentIdList([]), new List<OffsetInfo>(), new List<int>());
            }

            documentIdsWithOffsets.DocumentIds.Add(internalDocumentId);

            OffsetInfo.CreateOffsetInfo(tokenOffsets, documentIdsWithOffsets.OffsetInfos,
                documentIdsWithOffsets.Offsets);
        }
    }
}
