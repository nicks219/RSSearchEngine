using SearchEngine.Tokenizer.Dto;

namespace SearchEngine.Tokenizer.TokenizerProcessor;

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

    /// <summary>
    /// Вычислить метрику сравнения двух векторов, для эталонного вектора на основе расширенного набора.
    /// Учитывается последовательность токенов (т.е. "слов").
    /// </summary>
    /// <param name="targetVector">Вектор, в котором ищем.</param>
    /// <param name="searchVector">Вектор, который ищем.</param>
    /// <param name="searchStartIndex">Позиция, с которой начинать анализ по вектору с поисковым запросом.</param>
    /// <returns>Метрика количества совпадений.</returns>
    public override int ComputeComparisonScore(TokenVector targetVector, TokenVector searchVector, int searchStartIndex = 0)
    {
        // NB "облака лошадки без оглядки облака лошадки без оглядки" в 227 и 270 = 5

        var comparisonScore = 0;

        var startIndex = 0;

        for (var index = (uint)searchStartIndex; index < searchVector.Count; index++)
        {
            var hash = searchVector.ElementAt(index);
            var token = new Token(hash);
            var intersectionIndex = targetVector.IndexOf(token, startIndex);
            if (intersectionIndex == -1)
            {
                continue;
            }

            comparisonScore++;

            startIndex = intersectionIndex + 1;

            if (startIndex >= targetVector.Count)
            {
                break;
            }
        }

        return comparisonScore;
    }
}
