using System;
using System.Collections.Generic;
using System.Linq;

namespace RsseEngine.Dto;

/// <summary>
/// Вектор из хэшей, представляющий собой токенизированную заметку.
/// </summary>
/// <param name="tokens">Токенизированная заметка.</param>
public readonly struct TokenVector(List<int> tokens) : IEquatable<TokenVector>
{
    // Токенизированная заметка.
    private readonly List<int> _tokens = tokens;

    /// <summary>
    /// Получить количество токенов, содержащихся в векторе.
    /// </summary>
    public int Count => _tokens.Count;

    /// <summary>
    /// Определить, содержит ли вектор токен.
    /// </summary>
    /// <param name="token">Токен.</param>
    /// <returns><b>true</b> - Вектор содержит токен.</returns>
    public bool Contains(Token token) => _tokens.Contains(token.Value);

    /// <summary>
    /// Вернуть отсчитываемый от ноля индекс первого вхождения токена.
    /// </summary>
    /// <param name="token">Токен.</param>
    /// <param name="startIndex">Отсчитываемый от ноля индекс начала поиска.</param>
    /// <returns>Отсчитываемый от ноля индекс первого вхождения токена, либо -1 если токен не найден.</returns>
    public int IndexOf(Token token, int startIndex) => _tokens.IndexOf(token.Value, startIndex);

    /// <summary>
    /// Получить перечислитель для вектора.
    /// </summary>
    /// <returns>Перечислитель для вектора.</returns>
    public Enumerator GetEnumerator() => new(_tokens.GetEnumerator());

    public bool Equals(TokenVector other) => _tokens.Equals(other._tokens);

    public override bool Equals(object? obj) => obj is TokenVector other && Equals(other);

    public override int GetHashCode() => _tokens.GetHashCode();

    public static bool operator ==(TokenVector left, TokenVector right) => left.Equals(right);

    public static bool operator !=(TokenVector left, TokenVector right) => !(left == right);

    /// <summary>
    /// Получить по индексу токен из вектора.
    /// </summary>
    /// <param name="index">Индекс.</param>
    /// <returns>Токен.</returns>
    public Token ElementAt(int index) => new(_tokens[index]);

    /// <summary>
    /// Получить копию вектора как коллекцию хэшей.
    /// Для целей тестирования.
    /// </summary>
    /// <returns>Коллекция хэшей.</returns>
    public List<int> ToIntList() => _tokens.ToList();

    public Dictionary<Token, List<int>> ToDictionary()
    {
        var dictionary = new Dictionary<Token, List<int>>();

        for (int index = 0; index < _tokens.Count; index++)
        {
            var token = new Token(_tokens[index]);

            if (!dictionary.TryGetValue(token, out var offsets))
            {
                offsets = new List<int>();
                dictionary.Add(token, offsets);
            }

            offsets.Add(index);
        }

        return dictionary;
    }

    /// <summary>
    /// Конвертировать в вектор с уникальными элементами.
    /// </summary>
    /// <returns>Вектор с уникальными токенами.</returns>
    public TokenVector DistinctAndGet() => new(_tokens.ToHashSet().ToList());

    /// <summary>
    /// Перечислитель для вектора.
    /// </summary>
    public struct Enumerator(List<int>.Enumerator enumerator)
    {
        private List<int>.Enumerator _enumerator = enumerator;

        public bool MoveNext() => _enumerator.MoveNext();

        public Token Current => new(_enumerator.Current);
    }
}
