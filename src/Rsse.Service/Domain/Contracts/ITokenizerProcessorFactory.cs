using SearchEngine.Domain.Tokenizer;
using SearchEngine.Domain.Tokenizer.Processor;

namespace SearchEngine.Domain.Contracts;

/// <summary>
/// Контракт фабрики по созданию обработчиков векторов.
/// </summary>
public interface ITokenizerProcessorFactory
{
    /// <summary>
    /// Создать процессор токенизации требуемого типа.
    /// </summary>
    /// <param name="processorType">Тип процессора.</param>
    /// <returns>Процессор токенизации.</returns>
    ITokenizerProcessor CreateProcessor(ProcessorType processorType);
}
