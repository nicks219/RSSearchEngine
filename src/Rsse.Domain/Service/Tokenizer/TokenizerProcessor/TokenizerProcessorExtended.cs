namespace SearchEngine.Service.Tokenizer.TokenizerProcessor;

/// <summary>
/// Основной функционал токенизатора с расширенным набором символов.
/// </summary>
public sealed class TokenizerProcessorExtended : TokenizerProcessorBase
{
    // символ для увеличения поискового веса при вычислении индекса, используется для точного совпадения.
    private const string WeightExtendedChainSymbol = "@";
    // числовые символы.
    private const string Numbers = "0123456789";
    // дополненный набор символов из английского алфавита может включать: "eyuioa".
    // полностью сформированный расширенный набор символов для токенизации, может включать: "ёъь".
    private const string ExtendedConsonantChain =
        "цкнгшщзхфвпрлджчсмтб" + "яыоайуеиюэ" + ReducedEnglish + Numbers + WeightExtendedChainSymbol;

    /// <inheritdoc/>
    protected override string ConsonantChain => ExtendedConsonantChain;
}
