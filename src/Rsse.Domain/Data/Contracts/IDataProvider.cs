using System.Collections.Generic;

namespace SearchEngine.Data.Contracts;

/// <summary>
/// Контракт асинхронного источника данных.
/// </summary>
public interface IDataProvider<out T>
{
    /// <summary>
    /// Асинхронно отдавать последовательность данных.
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    IAsyncEnumerable<T> GetDataAsync();
}
