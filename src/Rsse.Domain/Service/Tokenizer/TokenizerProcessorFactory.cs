using System;
using SearchEngine.Service.Contracts;
using SearchEngine.Service.Tokenizer.Processor;

namespace SearchEngine.Service.Tokenizer;

/// <summary>
/// Фабрика по созданию обработчиков векторов.
/// </summary>
public class TokenizerProcessorFactory : ITokenizerProcessorFactory
{
    private readonly ProcessorReduced _reducedProcessor = new();
    private readonly ProcessorExtended _extendedProcessor = new();

    /// <inheritdoc/>
    /// <exception cref="ArgumentOutOfRangeException">Неизвестный тип процессора.</exception>
    public ITokenizerProcessor CreateProcessor(ProcessorType processorType)
    {
        return processorType switch
        {
            ProcessorType.Reduced => _reducedProcessor,
            ProcessorType.Extended => _extendedProcessor,
            _ => throw new ArgumentOutOfRangeException(nameof(processorType), processorType, null)
        };
    }
}
