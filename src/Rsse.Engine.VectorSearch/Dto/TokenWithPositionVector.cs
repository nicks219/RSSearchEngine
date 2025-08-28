using System;
using System.Collections.Generic;

namespace RsseEngine.Dto;

/// <summary>
/// Вектор из хэшей, представляющий собой токенизированную заметку.
/// </summary>
/// <param name="tokens">Токенизированная заметка.</param>
public readonly struct TokenWithPositionVector(List<TokenWithPosition> tokens) : IEquatable<TokenWithPositionVector>
{
    // Токенизированная заметка.
    private readonly List<TokenWithPosition> _tokens = tokens;

    public int Count => _tokens.Count;

    /// <summary>
    /// Получить перечислитель для вектора.
    /// </summary>
    /// <returns>Перечислитель для вектора.</returns>
    public List<TokenWithPosition>.Enumerator GetEnumerator() => _tokens.GetEnumerator();

    public bool Equals(TokenWithPositionVector other) => _tokens.Equals(other._tokens);

    public override bool Equals(object? obj) => obj is TokenWithPositionVector other && Equals(other);

    public override int GetHashCode() => _tokens.GetHashCode();

    public static bool operator ==(TokenWithPositionVector left, TokenWithPositionVector right) => left.Equals(right);

    public static bool operator !=(TokenWithPositionVector left, TokenWithPositionVector right) => !(left == right);
}
