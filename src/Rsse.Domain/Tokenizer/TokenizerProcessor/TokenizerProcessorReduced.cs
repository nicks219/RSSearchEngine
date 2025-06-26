using SearchEngine.Tokenizer.Dto;

namespace SearchEngine.Tokenizer.TokenizerProcessor;

/// <summary>
/// Основной функционал токенизатора с урезанным набором символов.
/// </summary>
public sealed class TokenizerProcessorReduced : TokenizerProcessorBase
{
    // полностью сформированный сокращенный набор символов для токенизации, может включать: "яыоайуеиюэъьё".
    private const string ReducedConsonantChain = "цкнгшщзхфвпрлджчсмтб" + ReducedEnglish;

    /// <inheritdoc/>
    protected override string ConsonantChain => ReducedConsonantChain;

    /// <summary>
    /// Вычислить метрику сравнения двух векторов, для эталонного вектора на основе редуцированного набора.
    /// Последовательность токенов (т.е. "слов") не учитывается.
    /// </summary>
    /// <param name="targetVector">Вектор, в котором ищем.</param>
    /// <param name="searchVector">Вектор, который ищем.</param>
    /// <param name="_">Не используется.</param>
    /// <returns>Метрика количества совпадений.</returns>
    public override int ComputeComparisonScore(TokenVector targetVector, TokenVector searchVector, int _ = 0)
    {
        // NB "я ты он она я ты он она я ты он она" будет найдено почти во всех заметках, необходимо обработать результат

        var comparisionScore = 0;
        foreach (var token in searchVector)
        {
            if (targetVector.Contains(token))
            {
                comparisionScore++;
            }
        }

        return comparisionScore;
    }
}
