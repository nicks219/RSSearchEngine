using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SearchEngine.Service.Tokenizer.Dto;

namespace SearchEngine.Service.Tokenizer;

/// <summary>
/// Поддержка общего инвертированного индекса.
/// </summary>
public class GinHandler
{
    /// <summary>
    /// Инвертированный индекс: токен в качестве ключа, идентификаторы заметок в качестве значения.
    /// </summary>
    private readonly Dictionary<Token, HashSet<DocId>> _generalInvertedIndex = [];

    /// <summary>
    /// Добавить в индекс вектор токенов и идентификатор заметки, в которой он встречается.
    /// </summary>
    /// <param name="vector">Вектор токенов.</param>
    /// <param name="id">Идентификатор заметки.</param>
    public void AddVector(TokenVector vector, DocId id)
    {
        foreach (var token in vector)
        {
            AddToken(token, id);
        }
    }

    /// <summary>
    /// Получить идентификаторы заметок, в которых присутствует токен.
    /// </summary>
    /// <param name="token">Токен.</param>
    /// <param name="ids">Сет с идентификаторами заметок.</param>
    /// <returns>Признак наличия токена в индексе.</returns>
    public bool TryGetIdentifiers(Token token, [MaybeNullWhen(false)] out HashSet<DocId> ids) =>
        _generalInvertedIndex.TryGetValue(token, out ids);

    /// <summary>
    /// Определить, присутствует ли любой токен из вектора в заданной заметке.
    /// </summary>
    /// <param name="vector">Вектор токенов.</param>
    /// <param name="id">Идентификатор заметки, для которой мы проверяем наличие любого токена из вектора.</param>
    /// <returns><b>true</b> - Один из токенов присутствует в заданной заметке.</returns>
    public bool ContainsAnyTokenForDoc(TokenVector vector, DocId id)
    {
        foreach (var token in vector)
        {
            if (!_generalInvertedIndex.TryGetValue(token, out var ids))
            {
                continue;
            }

            if (ids.Contains(id))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Добавить в индекс токен и идентификатор заметки, в которой он встречается.
    /// </summary>
    /// <param name="token">Токен.</param>
    /// <param name="id">Идентификатор заметки.</param>
    private void AddToken(Token token, DocId id)
    {
        if (!_generalInvertedIndex.TryGetValue(token, out var ids))
        {
            _generalInvertedIndex[token] = [id];
        }
        else
        {
            ids.Add(id);
        }
    }

    /// <summary>
    /// Определить, присутствует ли токен в любой из заданных заметок.
    /// </summary>
    /// <param name="token">Токен.</param>
    /// <param name="ids">Идентификаторы заметок, в которых мы проверяем наличие токена.</param>
    /// <returns><b>true</b> - Токен присутствует в одной из заданных заметок.</returns>
    // варианты нейминга: ContainsInAny | ContainsTokenInAnyGivenIds
    [Obsolete("в данный момент не используется")]
    public bool ContainsAnyDocForToken(Token token, HashSet<DocId> ids)
    {
        foreach (var id in ids)
        {
            if (!_generalInvertedIndex.TryGetValue(token, out var ginIds))
            {
                continue;
            }

            if (ginIds.Contains(id))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Удалить идентификатор заметки (и токен если сет останется пустым) из индекса.
    /// </summary>
    /// <param name="id">Идентификатор заметки.</param>
    public void RemoveId(DocId id) => throw new NotImplementedException();

    /// <summary>
    /// Обновить "заметку" (удалить + добавить).
    /// </summary>
    /// /// <param name="id">Идентификатор заметки.</param>
    /// <param name="vector">Вектор токенов, соответсвующий обновленной заметке.</param>
    public void UpdateId(DocId id, TokenVector vector) => throw new NotImplementedException();
}
