using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SearchEngine.Service.Tokenizer.Processor;

/// <summary>
/// Основной функционал токенизатора с расширенным набором симвлолв.
/// </summary>
public sealed class ProcessorExtended : ProcessorBase
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
    /// Учитывается последовательность токенов (т.е. "слов")
    /// </summary>
    /// <param name="referenceTokens">эталонный вектор</param>
    /// <param name="inputTokens">сравниваемый вектор</param>
    /// <returns>метрика количества совпадений</returns>
    public override int ComputeComparisionMetric(List<int> referenceTokens, List<int> inputTokens)
    {
        // NB "облака лошадки без оглядки облака лошадки без оглядки" в 227 и 270 = 5

        var result = 0;

        var startIndex = 0;

        foreach (var intersectionIndex in inputTokens.Select(value => referenceTokens.IndexOf(value, startIndex))
                     .Where(intersectionIndex => intersectionIndex != -1 && intersectionIndex >= startIndex))
        {
            result++;

            startIndex = intersectionIndex + 1;

            if (startIndex >= referenceTokens.Count)
            {
                break;
            }
        }

        return result;
    }
}
