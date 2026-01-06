using System.Collections.Generic;
using System.Runtime.InteropServices;
using RsseEngine.Dto.Common;
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

        AppendTokenVector(internalDocumentId, tokenVector);

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
        _internalDocumentIdToDocumentId.Clear();
        _documentIdToInternalDocumentId.Clear();
        _deletedDocuments.Clear();
    }

    /// <summary>
    /// Получить идентификаторы заметок, в которых присутствует токен.
    /// </summary>
    /// <param name="token">Токен.</param>
    /// <param name="documentDocumentIdsVector">Вектор с идентификаторами заметок.</param>
    /// <returns>Признак наличия токена в индексе.</returns>
    public bool TryGetNonEmptyDocumentIdVector(Token token, out DocumentIdsWithOffsets documentDocumentIdsVector)
    {
        if (!_invertedIndex.TryGetValue(token, out var documentIds) || documentIds.DocumentIds.Count == 0)
        {
            documentDocumentIdsVector = default;
            return false;
        }

        documentDocumentIdsVector = documentIds;
        return true;
    }

    public bool TryGetExternalDocumentId(InternalDocumentId documentId, out ExternalDocumentIdWithSize externalDocument)
    {
        if (_deletedDocuments.Contains(documentId))
        {
            externalDocument = default;
            return false;
        }

        externalDocument = _internalDocumentIdToDocumentId[documentId.Value];
        return true;
    }

    private void AppendTokenVector(InternalDocumentId internalDocumentId, TokenVector tokenVector)
    {
        var dictionary = tokenVector.ToPositionVector();

        foreach (var (token, tokenOffsets) in dictionary)
        {
            ref var documentIdsWithOffsets = ref CollectionsMarshal.GetValueRefOrAddDefault(
                _invertedIndex, token, out var exists);

            if (!exists)
            {
                documentIdsWithOffsets = new DocumentIdsWithOffsets(
                    new InternalDocumentIds([]), new List<OffsetInfo>(), new List<int>());
            }

            documentIdsWithOffsets.DocumentIds.Add(internalDocumentId);

            OffsetInfo.CreateOffsetInfo(tokenOffsets, documentIdsWithOffsets.OffsetInfos,
                documentIdsWithOffsets.Offsets);
        }
    }
}
