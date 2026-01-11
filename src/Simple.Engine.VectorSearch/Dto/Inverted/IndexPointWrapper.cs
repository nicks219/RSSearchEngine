using System;
using System.Runtime.CompilerServices;
using SimpleEngine.Dto.Common;

namespace SimpleEngine.Dto.Inverted;

/// <summary>
/// Элемент дополнительного индекса, представляет собой обертку над оптимизированными данными с заметкой.
/// </summary>
/// <param name="tokens">Оптимизированные данные с заметкой.</param>
public readonly struct IndexPointWrapper(IndexPoint tokens) : IEquatable<IndexPointWrapper>
{
    // Токенизированная заметка.
    private readonly IndexPoint _tokens = tokens;

    /// <summary>
    /// Получить количество токенов, содержащихся в векторе.
    /// </summary>
    public int Count => _tokens.Count;

    public bool Equals(IndexPointWrapper other) => _tokens.Equals(other._tokens);

    public override bool Equals(object? obj) => obj is IndexPointWrapper other && Equals(other);

    public override int GetHashCode() => _tokens.GetHashCode();

    public static bool operator ==(IndexPointWrapper left, IndexPointWrapper right) => left.Equals(right);

    public static bool operator !=(IndexPointWrapper left, IndexPointWrapper right) => !(left == right);

    /// <summary>
    ///
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKeyLinearScan(Token token)
    {
        return _tokens.ContainsKeyLinearScan(token.Value);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKeyBinarySearch(Token token)
    {
        return _tokens.ContainsKeyBinarySearch(token.Value);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="token"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindNextTokenPositionLinearScan(Token token, ref int position)
    {
        return _tokens.TryFindNextPositionLinearScan(token.Value, ref position);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="token"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFindNextTokenPositionBinarySearch(Token token, ref int position)
    {
        return _tokens.TryFindNextPositionBinarySearch(token.Value, ref position);
    }
}
