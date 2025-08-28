using System;

namespace RsseEngine.Dto;

/// <summary>
/// Хэш, представляющий токенизированное слово в векторе.
/// </summary>
/// <param name="token">Токенизированное слово.</param>
public readonly struct TokenWithPosition(Token token, int position) : IEquatable<TokenWithPosition>, IComparable<TokenWithPosition>
{
    // Токенизированное слово.
    private readonly Token _token = token;

    private readonly int _position = position;

    public Token Token => _token;

    public int Position => _position;

    public bool Equals(TokenWithPosition other) => _token.Equals(other._token) && _position.Equals(other._position);

    public override bool Equals(object? obj) => obj is TokenWithPosition other && Equals(other);

    public override int GetHashCode() => _token.GetHashCode();

    public static bool operator ==(TokenWithPosition left, TokenWithPosition right) => left.Equals(right);

    public static bool operator !=(TokenWithPosition left, TokenWithPosition right) => !(left == right);

    public int CompareTo(TokenWithPosition other)
    {
        var tokeComparision = _token.CompareTo(other._token);

        return tokeComparision == 0 ? tokeComparision : _position.CompareTo(other._position);
    }

    public override string ToString()
    {
        return $"Token {_token} Position {_position}";
    }
}
