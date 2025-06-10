namespace SearchEngine.Service.Tokenizer.TokenizerProcessor;

/// <summary>
/// Контейнер, вычисляющий хэш на последовательность символов по мере их добавления.
/// </summary>
internal struct SequenceHashProcessor()
{
    private const int Factor = 31;

    private int _hash = 0;

    private int _tempFactor = Factor;

    private bool _hasValue;

    /// <summary>
    /// Добавить символ к последовательности, по которой вычисляется хэш.
    /// </summary>
    /// <param name="letter"></param>
    public void AddChar(char letter)
    {
        _hash += letter * _tempFactor;

        _tempFactor *= Factor;

        _hasValue = true;
    }

    /// <summary>
    /// Получить текущий хэш на добавленную последовательность символов.
    /// </summary>
    /// <returns>Хэш.</returns>
    public int GetHash() => _hash;

    /// <summary>
    /// Признак наличия добавленных символов в контейнере.
    /// </summary>
    /// <returns><b>true</b> - символы были добавлены.</returns>
    public bool HasValue() => _hasValue;
}
