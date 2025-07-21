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
        var dictionary = TokenizeVector(tokenVector);

        InternalDocumentId internalDocumentId = new InternalDocumentId(_documentIdCounter++);

        RemoveVector(documentId);

        _documentIdToInternalDocumentId[documentId] = internalDocumentId;
        _internalDocumentIdToDocumentId[internalDocumentId] = documentId;

        AppendTokenVector(dictionary, internalDocumentId);
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

    public bool TryGetExternalDocumentId(InternalDocumentId internalDocumentId, out DocumentId externalDocumentId)
    {
        return _internalDocumentIdToDocumentId.TryGetValue(internalDocumentId, out externalDocumentId);
    }

    private static Dictionary<Token, List<int>> TokenizeVector(TokenVector tokenVector)
    {
        Dictionary<Token, List<int>> dictionary = new Dictionary<Token, List<int>>();

        for (int index = 0; index < tokenVector.Count; index++)
        {
            Token token = tokenVector.ElementAt(index);

            if (!dictionary.TryGetValue(token, out var offsets))
            {
                offsets = new List<int>();
                dictionary.Add(token, offsets);
            }

            offsets.Add(index);
        }

        return dictionary;
    }

    private void AppendTokenVector(Dictionary<Token, List<int>> dictionary, InternalDocumentId internalDocumentId)
    {
        foreach (var (token, offsets) in dictionary)
        {
            ref DocumentIdsWithOffsets collection = ref CollectionsMarshal.GetValueRefOrAddDefault(
                _invertedIndex, token, out bool exists);

            if (!exists)
            {
                collection = new DocumentIdsWithOffsets(new DocumentIdOffsetList([]), new List<OffsetInfo>(), new List<int>());
            }

            collection.DocumentIds.Add(internalDocumentId);

            // Оптимизируем хранение позиций токенов - если позиций больще двух то храним в Offsets,
            // иначе храним позиции в OffsetInfos - первую позицию в Size вторую позицию OffsetIndex.
            // Позиции в OffsetInfos храним как отрицательные,
            // если Size отрицательный или ноль - то это первая позиция,
            // если OffsetIndex отрицательный - то это вторая позиция,
            // если Size больще ноля - позиции хранятся в Offsets.
            if (offsets.Count > 2)
            {
                int position = collection.Offsets.Count;

                collection.Offsets.AddRange(offsets);
                collection.OffsetInfos.Add(new OffsetInfo(offsets.Count, position));
            }
            else if (offsets.Count == 2)
            {
                collection.OffsetInfos.Add(new OffsetInfo(-offsets[0], -offsets[1]));
            }
            else
            {
                collection.OffsetInfos.Add(new OffsetInfo(-offsets[0], 0));
            }
        }
    }
}
