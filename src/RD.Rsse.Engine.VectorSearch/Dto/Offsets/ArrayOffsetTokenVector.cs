using System;
using System.Runtime.CompilerServices;

namespace RD.RsseEngine.Dto.Offsets;

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

    public bool Equals(ArrayOffsetTokenVector other) => _tokens.Equals(other._tokens);

    public override bool Equals(object? obj) => obj is ArrayOffsetTokenVector other && Equals(other);

    public override int GetHashCode() => _tokens.GetHashCode();

    public static bool operator ==(ArrayOffsetTokenVector left, ArrayOffsetTokenVector right) => left.Equals(right);

    public static bool operator !=(ArrayOffsetTokenVector left, ArrayOffsetTokenVector right) => !(left == right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKeyLinearScan(Token token)
    {
        return _tokens.ContainsKeyLinearScan(token.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKeyBinarySearch(Token token)
    {
        return _tokens.ContainsKeyBinarySearch(token.Value);
    }

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindTokenCountLinearScan(Token token, out int count)
    {
        return _tokens.TryFindTokenCountLinearScan(token.Value, out count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindTokenCountBinarySearch(Token token, out int count)
    {
        return _tokens.TryFindTokenCountBinarySearch(token.Value, out count);
    }
}
