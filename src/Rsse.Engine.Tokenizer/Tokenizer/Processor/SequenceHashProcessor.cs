namespace RsseEngine.Tokenizer.Processor;

/// <summary>
/// Вычисление хэша на последовательность символов по мере их добавления.
/// </summary>
internal struct SequenceHashProcessor()
{
    private const int Factor = 31;

    private int _hash = 0;

    private int _tempFactor = Factor;

    private bool _hasValue;

    private void Reset()
    {
        _hash = 0;
        _tempFactor = Factor;
        _hasValue = false;
    }

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
    /// Получить текущий хэш на добавленную последовательность символов,
    /// подготовить контейнер для повторного использования.
    /// </summary>
    /// <returns>Хэш.</returns>
    public int GetHashAndReset()
    {
        var temp = _hash;
        Reset();
        return temp;
    }

    /// <summary>
    /// Признак наличия добавленных символов в контейнере.
    /// </summary>
    /// <returns><b>true</b> - символы были добавлены.</returns>
    public bool HasValue() => _hasValue;
}
