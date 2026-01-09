# if IS_RD_PROJECT

using System.Collections.Generic;
using System.Runtime.InteropServices;
using SimpleEngine.Dto.Common;
using SimpleEngine.Dto.Offsets;

namespace SimpleEngine.Indexes;

/// <summary>
/// Поддержка общего инвертированного индекса "токен-идентификаторы.
/// </summary>
public sealed class InvertedOffsetIndex
{
    /// <summary>
    /// Инвертированный индекс: токен в качестве ключа, набор идентификаторов заметок в качестве значения.
    /// </summary>
    private readonly Dictionary<Token, InternalDocumentIdsWithOffsets> _invertedIndex = new();

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

        AppendTokenVector(internalDocumentId, tokenVector);

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
        _externalDocumentIds.Clear();
        _actualDocuments.Clear();
        _deletedDocuments.Clear();
    }

    /// <summary>
    /// Получить идентификаторы заметок, в которых присутствует токен.
    /// </summary>
    /// <param name="token">Токен.</param>
    /// <param name="internalDocumentIdsWithOffsets">Вектор с идентификаторами заметок.</param>
    /// <returns>Признак наличия токена в индексе.</returns>
    public bool TryGetNonEmptyDocumentIdVector(Token token, out InternalDocumentIdsWithOffsets internalDocumentIdsWithOffsets)
    {
        if (!_invertedIndex.TryGetValue(token, out var documentIds) || documentIds.DocumentIds.Count == 0)
        {
            internalDocumentIdsWithOffsets = default;
            return false;
        }

        internalDocumentIdsWithOffsets = documentIds;
        return true;
    }

    public bool TryGetExternalDocumentId(InternalDocumentId documentId, out ExternalDocumentIdWithSize externalDocument)
    {
        if (_deletedDocuments.Contains(documentId))
        {
            externalDocument = default;
            return false;
        }

        externalDocument = _externalDocumentIds[documentId.Value];
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
                documentIdsWithOffsets = new InternalDocumentIdsWithOffsets(
                    new InternalDocumentIds([]), new List<OffsetInfo>(), new List<int>());
            }

            documentIdsWithOffsets.DocumentIds.Add(internalDocumentId);

            OffsetInfo.CreateOffsetInfo(tokenOffsets, documentIdsWithOffsets.OffsetInfos,
                documentIdsWithOffsets.Offsets);
        }
    }
}

#endif
