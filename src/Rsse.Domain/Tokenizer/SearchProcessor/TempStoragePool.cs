using System.Collections.Generic;
using Microsoft.Extensions.ObjectPool;
using SearchEngine.Tokenizer.Dto;

namespace SearchEngine.Tokenizer.SearchProcessor;

/// <summary>
/// Контейнер с пулами для временных коллекций.
/// </summary>
internal static class TempStoragePool
{
    /// <summary>
    /// Размер временных коллекций при инициализации.
    /// Точное значение заранее неизвестно, начать можно с 1% от общего количества заметок.
    /// </summary>
    internal const int StartTempStorageCapacity = 500;

    /// <summary>
    /// Пул для временных reduced-метрик.
    /// </summary>
    internal static readonly DefaultObjectPool<Dictionary<DocId, int>> ScoresTempStorage =
        new(new DefaultPooledObjectPolicy<Dictionary<DocId, int>>());

    /// <summary>
    /// Пул для временного extended-пространства поиска.
    /// </summary>
    internal static readonly DefaultObjectPool<List<DocIdVector>> VectorsTempStorage =
        new(new DefaultPooledObjectPolicy<List<DocIdVector>>());
}
