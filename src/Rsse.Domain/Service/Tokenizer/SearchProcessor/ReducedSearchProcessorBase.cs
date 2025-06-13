using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using SearchEngine.Service.Tokenizer.Contracts;
using SearchEngine.Service.Tokenizer.Dto;

namespace SearchEngine.Service.Tokenizer.SearchProcessor;

/// <summary>
/// Базовый класс для функционала алгоритмов подсчёта reduced метрик.
/// </summary>
public class ReducedSearchProcessorBase : IDisposable
{
    // Не забудь очистить при остановке приложения.
    /// <summary>
    /// Тредлокал для временных reduced-метрик.
    /// </summary>
    protected static readonly ThreadLocal<Dictionary<DocId, int>> ScoresStorage =
        new(() => new(IExtendedSearchProcessor.StartTempStorageCapacity));

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
    public void Dispose() => ScoresStorage.Dispose();
}
