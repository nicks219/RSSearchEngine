using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using SimpleEngine.Dto.Common;
using SimpleEngine.Dto.Inverted;

namespace SimpleEngine.Indexes;

/// <summary>
/// Контейнер с поддержкой нескольких индексов (общего и обратного).
/// </summary>
public sealed class CommonIndex(IndexPoint.DictionaryStorageType dataPointSearchType)
{
    /// <summary>
    /// Инвертированный индекс: токен в качестве ключа, набор идентификаторов заметок в качестве значения.
    /// </summary>
    private readonly Dictionary<Token, InternalDocumentIds> _invertedIndex = new();

    private readonly List<IndexPointWrapper> _directIndex = [];

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
        if (_actualDocuments.Remove(documentId, out var oldDocumentIdInternal))
        {
            _deletedDocuments.Add(oldDocumentIdInternal);
        }
    }

    /// <summary>
    /// Очистить индексы.
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
    /// Получить из индекса идентификаторы заметок, в которых присутствует токен.
    /// Используется в RelevanceFilter.
    /// </summary>
    /// <param name="token">Токен.</param>
    /// <param name="documentIdsInternal">Вектор с идентификаторами заметок из индекса.</param>
    /// <returns>Признак наличия токена в индексе.</returns>
    public bool TryGetNonEmptyDocumentIds(Token token, out InternalDocumentIds documentIdsInternal)
    {
        return _invertedIndex.TryGetValue(token, out documentIdsInternal) && documentIdsInternal.Count > 0;
    }

    /// <summary>
    /// Заполнить коллекцию с актуальными идентификаторами из индекса, которые соответствуют токенам из запрашиваемого вектора.
    /// </summary>
    /// <param name="tokens">Вектор с целевыми токенами.</param>
    /// <param name="documentIdsInternal">Коллекция токен-идентификаторы индекса.</param>
    public void FillWithNonEmptyDocumentIds(TokenVector tokens, List<InternalDocumentIdsWithToken> documentIdsInternal)
    {
        foreach (var token in tokens)
        {
            if (TryGetNonEmptyDocumentIds(token, out var documentIds))
            {
                documentIdsInternal.Add(new InternalDocumentIdsWithToken(documentIds, token));
            }
        }
    }

    /// <summary>
    /// Заполнить коллекцию актуальными (либо пустыми) идентификаторами из индекса, которые соответствуют токенам из запрашиваемого вектора.
    /// </summary>
    /// <param name="tokens">Вектор с целевыми токенами.</param>
    /// <param name="documentIdsInternal">Коллекция токен-идентификаторы индекса.</param>
    public void FillWithDocumentIds(TokenVector tokens, List<InternalDocumentIds> documentIdsInternal)
    {
        var emptyVector = new InternalDocumentIds([]);

        foreach (var token in tokens)
        {
            documentIdsInternal.Add(TryGetNonEmptyDocumentIds(token, out var documentIds)
                ? documentIds
                : emptyVector);
        }
    }

    /// <summary>
    /// Получить документ (если он не помечен к удалению) в виде коллекции токенов и их позиций.
    /// </summary>
    /// <param name="documentId">Идентификатор документа в индексе.</param>
    /// <param name="directIndexPoint">Контейнер с компактным вектором позиций токенов документа.</param>
    /// <param name="externalDocumentId">Идентификатор документа в бд.</param>
    /// <returns></returns>
    public bool TryGetPositionVector(
        InternalDocumentId documentId,
        out IndexPointWrapper directIndexPoint,
        out ExternalDocumentIdWithSize externalDocumentId)
    {
        if (_deletedDocuments.Contains(documentId))
        {
            directIndexPoint = default;
            externalDocumentId = default;
            return false;
        }

        directIndexPoint = _directIndex[documentId.Value];
        // externalDocument = new ExternalDocumentIdWithSize(new DocumentId(positionVector.Value.ExternalId), positionVector.Value.ExternalCount);
        externalDocumentId = _externalDocumentIds[documentId.Value];
        return true;
    }

    /// <summary>
    /// Добавить заметку в инвертированный индекс.
    /// </summary>
    /// <param name="externalDocumentId">Идентификатор документа в бд.</param>
    /// <param name="internalDocumentId">Идентификатор документа в индексе.</param>
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

        var indexPoint = new IndexPoint(
            positionVectorConverted,
            externalDocumentId.Value,
            tokenVector.Count,
            dataPointSearchType);

        var indexPointWrapper = new IndexPointWrapper(indexPoint);

        _directIndex.Add(indexPointWrapper);
    }
}
