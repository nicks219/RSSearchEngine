using System;
using SearchEngine.Service.Tokenizer.Contracts;
using SearchEngine.Service.Tokenizer.TokenizerProcessor;

namespace SearchEngine.Service.Tokenizer;

/// <summary>
/// Фабрика по созданию обработчиков векторов.
/// </summary>
public sealed class TokenizerProcessorFactory : ITokenizerProcessorFactory
{
    private readonly TokenizerProcessorReduced _reducedTokenizerProcessor = new();
    private readonly TokenizerProcessorExtended _extendedTokenizerProcessor = new();

    /// <inheritdoc/>
    /// <exception cref="ArgumentOutOfRangeException">Неизвестный тип процессора.</exception>
    public ITokenizerProcessor CreateProcessor(ProcessorType processorType)
    {
        return processorType switch
        {
            ProcessorType.Reduced => _reducedTokenizerProcessor,
            ProcessorType.Extended => _extendedTokenizerProcessor,
            _ => throw new ArgumentOutOfRangeException(nameof(processorType), processorType, null)
        };
    }
}
