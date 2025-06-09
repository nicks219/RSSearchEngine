using System;
using System.Collections.Generic;
using System.Linq;
using SearchEngine.Service.Tokenizer.Wrapper;

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
    /// Учитывается последовательность токенов (т.е. "слов").
    /// </summary>
    /// <param name="targetVector">Вектор, в котором ищем.</param>
    /// <param name="searchVector">Вектор, который ищем.</param>
    /// <returns>Метрика количества совпадений.</returns>
    public override int ComputeComparisionMetric(TokenVector targetVector, TokenVector searchVector)
    {
        // NB "облака лошадки без оглядки облака лошадки без оглядки" в 227 и 270 = 5

        var metrics = 0;

        var startIndex = 0;

        foreach (var token in searchVector)
        {
            var intersectionIndex = targetVector.IndexOf(token, startIndex);
            if (intersectionIndex != -1)
            {
                metrics++;

                startIndex = intersectionIndex + 1;

                if (startIndex >= targetVector.Count)
                {
                    break;
                }
            }
        }

        return metrics;
    }

    // Todo: метод ComputeComparisionMetric до внесения изменений.
    [Obsolete("Todo: зафиксировать поведение юнит-тестами, оригинальный метод до профилирования.")]
    public int ComputeComparisionMetric_Old(List<int> referenceTokens, List<int> inputTokens)
    {
        // NB "облака лошадки без оглядки облака лошадки без оглядки" в 227 и 270 = 5

        var result = 0;

        var startIndex = 0;

        foreach (var intersectionIndex in inputTokens
                     .Select(value => referenceTokens.IndexOf(value, startIndex))
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
