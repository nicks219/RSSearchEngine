using System;
using System.Collections.Generic;
using System.Linq;

namespace SearchEngine.Service.Tokenizer.Processor;

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
    /// Последовательность токенов (т.е. "слов") не учитывается.
    /// </summary>
    /// <param name="targetVector">Вектор, в котором ищем.</param>
    /// <param name="searchVector">Вектор, который ищем.</param>
    /// <returns>Метрика количества совпадений.</returns>
    public override int ComputeComparisionMetric(List<int> targetVector, List<int> searchVector)
    {
        // NB "я ты он она я ты он она я ты он она" будет найдено почти во всех заметках, необходимо обработать результат

        var metrics = 0;
        foreach (var token in searchVector)
        {
            if (targetVector.Contains(token))
            {
                metrics++;
            }
        }

        return metrics;
    }

    // Todo: метод ComputeComparisionMetric до внесения изменений.
    [Obsolete("Todo: зафиксировать поведение юнит-тестами, оригинальный метод до профилирования.")]
    public int ComputeComparisionMetric_Old(List<int> referenceTokens, IEnumerable<int> inputTokens)
    {
        // NB "я ты он она я ты он она я ты он она" будет найдено почти во всех заметках, необходимо обработать результат

        var result = referenceTokens.Intersect(inputTokens);

        return result.Count();
    }
}
