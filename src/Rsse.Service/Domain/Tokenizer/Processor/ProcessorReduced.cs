using System.Collections.Generic;
using System.Linq;

namespace SearchEngine.Domain.Tokenizer.Processor;

/// <summary>
/// Основной функционал токенизатора с урезанным набором символов.
/// </summary>
public sealed class ProcessorReduced : ProcessorBase
{
    // полностью сформированный сокращенный набор символов для токенизации, может включать: "яыоайуеиюэъьё".
    private const string ReducedConsonantChain = "цкнгшщзхфвпрлджчсмтб" + ReducedEnglish;

    /// <inheritdoc/>
    protected override string ConsonantChain => ReducedConsonantChain;

    /// <summary>
    /// Вычислить метрику сравнения двух векторов, для эталонного вектора на основе редуцированного набора.
    /// Последовательность токенов (т.е. "слов") не учитывается
    /// </summary>
    /// <param name="referenceTokens">эталонный вектор</param>
    /// <param name="inputTokens">сравниваемый вектор</param>
    /// <returns>метрика количества совпадений</returns>
    public override int ComputeComparisionMetric(List<int> referenceTokens, IEnumerable<int> inputTokens)
    {
        // NB "я ты он она я ты он она я ты он она" будет найдено почти во всех заметках, необходимо обработать результат

        var result = referenceTokens.Intersect(inputTokens);

        return result.Count();
    }
}
