using SearchEngine.Service.Tokenizer.Processor;

namespace SearchEngine.Service.Contracts;

/// <summary>
/// Контракт фабрики по созданию обработчиков векторов.
/// </summary>
public interface ITokenizerProcessorFactory
{
    /// <summary>
    /// Создать процессор требуемого типа для токенизации.
    /// </summary>
    /// <param name="processorType">Требуемый тип процессора.</param>
    /// <returns>Процессор токенизации.</returns>
    ITokenizerProcessor CreateProcessor(ProcessorType processorType);
}
