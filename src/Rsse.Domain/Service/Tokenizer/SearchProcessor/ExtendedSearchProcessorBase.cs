using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using SearchEngine.Service.Tokenizer.Contracts;
using SearchEngine.Service.Tokenizer.Dto;

namespace SearchEngine.Service.Tokenizer.SearchProcessor;

/// <summary>
/// Базовый класс для функционала алгоритмов подсчёта extended метрик.
/// </summary>
public abstract class ExtendedSearchProcessorBase : IDisposable
{
    /// <summary>
    /// Тредлокал для временного extended-пространства поиска.
    /// </summary>
    protected static readonly ThreadLocal<List<DocIdVector>> VectorsTempStorage = new(() => []);

    /// <summary>
    /// Фабрика токенизаторов.
    /// </summary>
    public required ITokenizerProcessorFactory TokenizerProcessorFactory { get; init; }

    /// <summary>
    /// Индекс для всех токенизированных заметок.
    /// </summary>
    public required ConcurrentDictionary<DocId, TokenLine> GeneralDirectIndex { get; init; }

    /// <summary>
    /// Закрываем локальный стор потоков.
    /// </summary>
    public void Dispose() => VectorsTempStorage.Dispose();
}
