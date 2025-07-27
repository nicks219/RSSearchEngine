using System;
using System.Collections.Generic;
using System.Linq;

namespace RsseEngine.Dto.Offsets;

/// <summary>
/// Вектор из хэшей, представляющий собой токенизированную заметку.
/// </summary>
/// <param name="tokens">Токенизированная заметка.</param>
public readonly struct OffsetTokenVector(List<int> tokens, List<OffsetInfo> offsetInfos, List<int> offsets)
    : IEquatable<OffsetTokenVector>
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
    /// Вернуть отсчитываемый от ноля индекс первого вхождения токена.
    /// </summary>
    /// <param name="token">Токен.</param>
    /// <returns>Отсчитываемый от ноля индекс первого вхождения токена, либо -1 если токен не найден.</returns>
    public int BinarySearch(Token token) => _tokens.BinarySearch(token.Value);

    /// <summary>
    /// Получить перечислитель для вектора.
    /// </summary>
    /// <returns>Перечислитель для вектора.</returns>
    public Enumerator GetEnumerator() => new(_tokens.GetEnumerator());

    /// <summary>
    /// Конвертировать в вектор с уникальными элементами.
    /// </summary>
    /// <returns>Вектор с уникальными токенами.</returns>
    public TokenVector DistinctAndGet() => new(_tokens.ToList());

    public bool Equals(OffsetTokenVector other) => _tokens.Equals(other._tokens);

    public override bool Equals(object? obj) => obj is OffsetTokenVector other && Equals(other);

    public override int GetHashCode() => _tokens.GetHashCode();

    public static bool operator ==(OffsetTokenVector left, OffsetTokenVector right) => left.Equals(right);

    public static bool operator !=(OffsetTokenVector left, OffsetTokenVector right) => !(left == right);

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

    public HashSet<Token> Intersect(OffsetTokenVector tokenVector)
    {
        return _tokens.Intersect(tokenVector._tokens)
            .Select(token => new Token(token))
            .ToHashSet();
    }

    public bool TryFindNextTokenPosition(Token token, ref int position)
    {
        //*
        var index = IndexOf(token, 0);
        if (index != -1)
        /*/
        var index = BinarySearch(token);
        if (index >= 0)
        //*/
        {
            var offsetInfo = offsetInfos[index];

            if (offsetInfo.TryFindNextPosition(offsets, ref position))
            {
                return true;
            }
        }

        return false;
    }

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
