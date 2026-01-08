using System.Collections.Generic;

namespace SimpleEngine.Tokenizer.Common;

/// <summary>
/// Пулы для коллекций, используемых токенайзером.
/// </summary>
public sealed class SimpleStoragePools
{
    /// <summary>
    /// Пул для списков от int.
    /// </summary>
    public readonly ConcurrentBagPool<List<int>> ListPool = new();

    /// <summary>
    /// Пул для множеств от int.
    /// </summary>
    public readonly ConcurrentBagPool<HashSet<int>> SetPool = new();
}
