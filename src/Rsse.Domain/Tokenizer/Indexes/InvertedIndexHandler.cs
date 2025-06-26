using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SearchEngine.Tokenizer.Dto;

namespace SearchEngine.Tokenizer.Indexes;

/// <summary>
/// Поддержка общего (инвертированного) индекса по токенам.
/// </summary>
public sealed class InvertedIndexHandler
{
    /// <summary>
    /// Инвертированный индекс: токен - идентификаторы заметок.
    /// </summary>
    private readonly Dictionary<Token, DocIdVector> _generalInvertedIndex = [];

    /// <summary>
    /// Добавить в индекс вектор токенов и идентификатор соответствующей ему заметки.
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
    /// <param name="docIdVector">Вектор с идентификаторами заметок.</param>
    /// <returns>Признак наличия токена в индексе.</returns>
    public bool TryGetIdentifiers(Token token, [MaybeNullWhen(false)] out DocIdVector docIdVector) =>
        _generalInvertedIndex.TryGetValue(token, out docIdVector);

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
            if (!_generalInvertedIndex.TryGetValue(token, out var docIdVector))
            {
                continue;
            }

            if (docIdVector.Contains(id))
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
        if (!_generalInvertedIndex.TryGetValue(token, out var docIdVector))
        {
            _generalInvertedIndex[token] = new DocIdVector([id]);
        }
        else
        {
            docIdVector.Add(id);
        }
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
