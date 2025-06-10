using System;
using System.Collections.Generic;
using System.Linq;

namespace SearchEngine.Service.Tokenizer.Dto;

/// <summary>
/// Вектор из хэшей, представляющий собой токенизированную заметку.
/// </summary>
/// <param name="vector">Токенизированная заметка.</param>
public readonly struct TokenVector(List<int> vector) : IEquatable<TokenVector>
{
    // Токенизированная заметка.
    private readonly List<int> _vector = vector;

    /// <summary>
    /// Получить количество токенов, содержащихся в векторе.
    /// </summary>
    public int Count => _vector.Count;

    /// <summary>
    /// Добавить хэш в вектор.
    /// </summary>
    /// <param name="hash">Хэш.</param>
    internal void Add(int hash) => _vector.Add(hash);

    /// <summary>
    /// Определить, содержит ли вектор токен.
    /// </summary>
    /// <param name="token">Токен.</param>
    /// <returns><b>true</b> - Вектор содержит токен.</returns>
    public bool Contains(Token token) => _vector.Contains(token.Value);

    /// <summary>
    /// Вернуть отсчитываемый от ноля индекс первого вхождения токена.
    /// </summary>
    /// <param name="token">Токен.</param>
    /// <param name="startIndex">Отсчитываемый от ноля индекс начала поиска.</param>
    /// <returns>Отсчитываемый от ноля индекс первого вхождения токена, либо -1 если токен не найден.</returns>
    public int IndexOf(Token token, int startIndex) => _vector.IndexOf(token.Value, startIndex);

    /// <summary>
    /// Получить перечислитель для вектора.
    /// </summary>
    /// <returns>Перечислитель для вектора.</returns>
    public Enumerator GetEnumerator() => new(_vector.GetEnumerator());

    /// <summary>
    /// Получить вектор как коллекцию хэшей.
    /// Для целей тестирования.
    /// </summary>
    /// <returns>Список хэшей.</returns>
    internal List<int> ToIntList() => _vector;

    /// <summary>
    /// Конвертировать в вектор с уникальными элементами.
    /// </summary>
    /// <returns>Вектор с уникальными токенами.</returns>
    public TokenVector DistinctAndGet() => new(_vector.ToHashSet().ToList());

    public bool Equals(TokenVector other) => _vector.Equals(other._vector);

    public override bool Equals(object? obj) => obj is TokenVector other && Equals(other);

    public override int GetHashCode() => _vector.GetHashCode();

    public static bool operator ==(TokenVector left, TokenVector right) => left.Equals(right);

    public static bool operator !=(TokenVector left, TokenVector right) => !(left == right);

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
