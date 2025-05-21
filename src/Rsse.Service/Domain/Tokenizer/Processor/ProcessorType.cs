namespace SearchEngine.Domain.Tokenizer.Processor;

/// <summary>
/// Тип процессора токенизации, определяется эталонной цепочкой символов и алгоритмом их обработки.
/// </summary>
public enum ProcessorType
{
    /// <summary>
    /// Тип обработчика для урезанной последовательность символов.
    /// </summary>
    Reduced = 0,

    /// <summary>
    /// Тип Обпвботчика для расширенной последовательности символов.
    /// </summary>
    Extended = 1
}
