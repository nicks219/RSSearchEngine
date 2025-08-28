using System.Collections.Generic;
using RsseEngine.Dto;

namespace RsseEngine.Tokenizer.Common;

/// <summary>
/// Пулы для коллекций, используемых токенайзером.
/// </summary>
public sealed class SimpleStoragePools
{
    /// <summary>
    /// Пул для списков от int.
    /// </summary>
    public readonly ConcurrentBagPool<List<int>, int> ListPool = new();

    /// <summary>
    /// Пул для множеств от int.
    /// </summary>
    public readonly ConcurrentBagPool<HashSet<int>, int> SetPool = new();

    public readonly ConcurrentBagPool<List<TokenWithPosition>, TokenWithPosition> TokenWithPositionListPool = new();
}
