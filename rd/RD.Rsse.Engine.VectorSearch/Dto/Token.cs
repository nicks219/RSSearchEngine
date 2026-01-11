using System;

namespace RD.RsseEngine.Dto;

/// <summary>
/// Хэш, представляющий токенизированное слово в векторе.
/// </summary>
/// <param name="token">Токенизированное слово.</param>
public readonly struct Token(int token) : IEquatable<Token>, IComparable<Token>
{
    // Токенизированное слово.
    private readonly int _token = token;

    /// <summary>
    /// Получить значение токена.
    /// </summary>
    // ReSharper disable once ConvertToAutoPropertyWhenPossible
    public int Value => _token;

    public bool Equals(Token other) => _token.Equals(other._token);

    public override bool Equals(object? obj) => obj is Token other && Equals(other);

    public override int GetHashCode() => _token.GetHashCode();

    public static bool operator ==(Token left, Token right) => left.Equals(right);

    public static bool operator !=(Token left, Token right) => !(left == right);

    public int CompareTo(Token other)
    {
        return _token.CompareTo(other._token);
    }

    public override string ToString()
    {
        return _token.ToString();
    }
}
