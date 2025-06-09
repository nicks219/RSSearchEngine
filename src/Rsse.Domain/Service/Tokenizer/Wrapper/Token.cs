using System;

namespace SearchEngine.Service.Tokenizer.Wrapper;

/// <summary>
/// Хэш, представляющий токенизированное слово в векторе.
/// </summary>
/// <param name="token">Токенизированное слово.</param>
public readonly struct Token(int token) : IEquatable<Token>
{
    // Токенизированное слово.
    private readonly int _token = token;

    /// <summary>
    /// Получить значение токена.
    /// </summary>
    // ReSharper disable once ConvertToAutoPropertyWhenPossible
    internal int Value => _token;

    public bool Equals(Token other) => _token.Equals(other._token);

    public override bool Equals(object? obj) => obj is Token other && Equals(other);

    public override int GetHashCode() => _token.GetHashCode();

    public static bool operator ==(Token left, Token right) => left.Equals(right);

    public static bool operator !=(Token left, Token right) => !(left == right);
}
