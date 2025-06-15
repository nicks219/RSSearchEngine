namespace SearchEngine.Service.Tokenizer.TokenizerProcessor;

/// <summary>
/// Тип процессора токенизации, определяет эталонную цепочку символов и алгоритм их обработки.
/// </summary>
public enum ProcessorType
{
    /// <summary>
    /// Тип обработчика для урезанной последовательность символов.
    /// </summary>
    Reduced = 0,

    /// <summary>
    /// Тип обработчика для расширенной последовательности символов.
    /// </summary>
    Extended = 1
}
