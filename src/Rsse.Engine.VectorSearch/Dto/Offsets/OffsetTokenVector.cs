using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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

    public bool Equals(OffsetTokenVector other) => _tokens.Equals(other._tokens);

    public override bool Equals(object? obj) => obj is OffsetTokenVector other && Equals(other);

    public override int GetHashCode() => _tokens.GetHashCode();

    public static bool operator ==(OffsetTokenVector left, OffsetTokenVector right) => left.Equals(right);

    public static bool operator !=(OffsetTokenVector left, OffsetTokenVector right) => !(left == right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindNextTokenPositionLinearScan(Token token, ref int position)
    {
        var index = _tokens.IndexOf(token.Value);
        if (index != -1)
        {
            var offsetInfo = offsetInfos[index];

            if (offsetInfo.TryFindNextPosition(offsets, ref position))
            {
                return true;
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindNextTokenPositionBinarySearch(Token token, ref int position)
    {
        var index = _tokens.BinarySearch(token.Value);
        if (index >= 0)
        {
            var offsetInfo = offsetInfos[index];

            if (offsetInfo.TryFindNextPosition(offsets, ref position))
            {
                return true;
            }
        }

        return false;
    }
}
