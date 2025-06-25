namespace SearchEngine.Service.Tokenizer.TokenizerProcessor;

/// <summary>
/// Основной функционал токенизатора с урезанным набором символов.
/// </summary>
public sealed class TokenizerProcessorReduced : TokenizerProcessorBase
{
    // полностью сформированный сокращенный набор символов для токенизации, может включать: "яыоайуеиюэъьё".
    private const string ReducedConsonantChain = "цкнгшщзхфвпрлджчсмтб" + ReducedEnglish;

    /// <inheritdoc/>
    protected override string ConsonantChain => ReducedConsonantChain;
}
