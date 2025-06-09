using System;
using System.Collections.Generic;
using System.Linq;

namespace SearchEngine.Service.Tokenizer.Wrapper;

/// <summary>
/// Вектор из хэшей, представляющий собой токенизированную заметку.
/// </summary>
/// <param name="vector">Токенизированная заметка.</param>
public readonly struct TokenVector(List<Token> vector) : IEquatable<TokenVector>
{
    /// <summary>
    /// Токенизированная заметка.
    /// </summary>
    private readonly List<Token> _vector = vector;

    /// <summary>
    /// Получить количество токенов, содержащихся в векторе.
    /// </summary>
    public int Count => _vector.Count;

    /// <summary>
    /// Определить, содержит ли вектор токен.
    /// </summary>
    /// <param name="token">Токен.</param>
    /// <returns><b>true</b> - Вектор содержит токен.</returns>
    public bool Contains(Token token) => _vector.Contains(token);

    /// <summary>
    /// Вернуть отсчитываемый от ноля индекс первого вхождения токена.
    /// </summary>
    /// <param name="token">Токен.</param>
    /// <param name="startIndex">Отсчитываемый от ноля индекс начала поиска.</param>
    /// <returns>Отсчитываемый от ноля индекс первого вхождения токена, либо -1 если токен не найден.</returns>
    public int IndexOf(Token token, int startIndex) => _vector.IndexOf(token, startIndex);

    /// <summary>
    /// Получить перечислитель для вектора.
    /// </summary>
    /// <returns>Перечислитель для вектора.</returns>
    public List<Token>.Enumerator GetEnumerator() => _vector.GetEnumerator();

    /// <summary>
    /// Конвертировать вектор в список int.
    /// </summary>
    /// <returns>Список с числами.</returns>
    public List<int> ToIntList() => _vector.ConvertAll(x => x.Value);

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
}
