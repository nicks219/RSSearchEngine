using System;
using System.Runtime.CompilerServices;

namespace RsseEngine.Dto.Offsets;

/// <summary>
/// Вектор из хэшей, представляющий собой токенизированную заметку.
/// </summary>
/// <param name="tokens">Токенизированная заметка.</param>
public readonly struct ArrayOffsetTokenVector(DocumentDataPoint tokens)
    : IEquatable<ArrayOffsetTokenVector>
{
    // Токенизированная заметка.
    private readonly DocumentDataPoint _tokens = tokens;

    /// <summary>
    /// Получить количество токенов, содержащихся в векторе.
    /// </summary>
    public int Count => _tokens.Count;

    public DocumentDataPoint Value => _tokens;

    /// <summary>
    /// Определить, содержит ли вектор токен.
    /// </summary>
    /// <param name="token">Токен.</param>
    /// <returns><b>true</b> - Вектор содержит токен.</returns>
    public bool Contains(Token token) => _tokens.ContainsKey(token.Value);

    public bool Equals(ArrayOffsetTokenVector other) => _tokens.Equals(other._tokens);

    public override bool Equals(object? obj) => obj is ArrayOffsetTokenVector other && Equals(other);

    public override int GetHashCode() => _tokens.GetHashCode();

    public static bool operator ==(ArrayOffsetTokenVector left, ArrayOffsetTokenVector right) => left.Equals(right);

    public static bool operator !=(ArrayOffsetTokenVector left, ArrayOffsetTokenVector right) => !(left == right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindNextTokenPositionLinearScan(Token token, ref int position)
    {
        return _tokens.TryFindNextPositionLinearScan(token.Value, ref position);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindNextTokenPositionBinarySearch(Token token, ref int position)
    {
        return _tokens.TryFindNextPositionBinarySearch(token.Value, ref position);
    }
}