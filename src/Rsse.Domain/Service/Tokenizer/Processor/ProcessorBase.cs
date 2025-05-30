using System;
using System.Collections.Generic;
using System.Linq;
using SearchEngine.Service.Contracts;

namespace SearchEngine.Service.Tokenizer.Processor;

/// <summary>
/// Базовый функционал токенизатора.
/// </summary>
public abstract class ProcessorBase : ITokenizerProcessor
{
    // Сокращенный набор символов из английского алфавита.
    protected const string ReducedEnglish = "qwrtpsdfghjklzxcvbnm";

    // Разделители слов в заметке.
    private static readonly char[] Separators = ['\r', '\n', ':', '/', '.', ' '];

    /// <summary>
    /// Полный набор символов для токенизации.
    /// </summary>
    protected abstract string ConsonantChain { get; }

    /// <inheritdoc/>
    public abstract int ComputeComparisionMetric(List<int> referenceTokens, IEnumerable<int> inputTokens);

    /// <inheritdoc/>
    public List<string> PreProcessNote(string note)
    {
        return note
            .ToLower()
            .Replace('ё', 'е')
            .Split(Separators, StringSplitOptions.RemoveEmptyEntries)
            .Select(word =>
                new string(word.Where(symbol => ConsonantChain.Contains(symbol)).ToArray()))
            .Where(word => word.Length > 0)
            .ToList();
    }

    /// <inheritdoc/>
    public List<int> TokenizeSequence(IEnumerable<string> strings)
    {
        const int factor = 31;

        var result = new List<int>();

        foreach (var word in strings)
        {
            var hash = 0;

            var tempFactor = factor;

            foreach (var letter in word)
            {
                hash += letter * tempFactor;

                tempFactor *= factor;
            }

            result.Add(hash);
        }

        return result;
    }
}
