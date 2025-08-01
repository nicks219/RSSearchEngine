using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace RsseEngine.Dto.Offsets;

/// <summary>
/// Вектор из хэшей, представляющий собой токенизированную заметку.
/// </summary>
/// <param name="tokens">Токенизированная заметка.</param>
public readonly struct FrozenOffsetTokenVector(FrozenDictionary<int, OffsetInfo> tokens, List<int> offsets)
    : IEquatable<FrozenOffsetTokenVector>
{
    // Токенизированная заметка.
    private readonly FrozenDictionary<int, OffsetInfo> _tokens = tokens;

    public bool Equals(FrozenOffsetTokenVector other) => _tokens.Equals(other._tokens);

    public override bool Equals(object? obj) => obj is FrozenOffsetTokenVector other && Equals(other);

    public override int GetHashCode() => _tokens.GetHashCode();

    public static bool operator ==(FrozenOffsetTokenVector left, FrozenOffsetTokenVector right) => left.Equals(right);

    public static bool operator !=(FrozenOffsetTokenVector left, FrozenOffsetTokenVector right) => !(left == right);

    public bool TryFindNextTokenPosition(Token token, ref int position)
    {
        return _tokens.TryGetValue(token.Value, out var offsetInfo)
               && offsetInfo.TryFindNextPosition(offsets, ref position);
    }
}
