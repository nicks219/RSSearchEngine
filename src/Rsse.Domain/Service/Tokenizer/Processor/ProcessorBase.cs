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
    private static readonly char[] Separators = ['\r', '\n', '\t', ':', '/', '.', ' '];

    /// <summary>
    /// Полный набор символов для токенизации.
    /// </summary>
    protected abstract string ConsonantChain { get; }

    /// <inheritdoc/>
    public abstract int ComputeComparisionMetric(List<int> referenceTokens, List<int> inputTokens);

    /// <inheritdoc/>
    public List<int> PreProcessNote(string note)
    {
        var list = new List<char>();
        var result = new List<int>();
        foreach (var word in Split())
        {
            foreach (var symbol in word)
            {
                if (ConsonantChain.Contains(symbol))
                {
                    list.Add(symbol);
                }
            }

            var preProcessedWord = list;

            if (preProcessedWord.Count > 0)
            {
                var hash = CreateHash(preProcessedWord);
                result.Add(hash);
                list.Clear();
            }
        }
        return result;

        /*return Split()
            .Select(word =>
                word.Where(symbol => ConsonantChain.Contains(symbol)).ToArray())
            .Where(word => word.Length > 0)
            .ToList();*/

        string[] Split()
        {
            return note
                .ToLower()
                .Replace('ё', 'е')
                .Split(Separators, StringSplitOptions.RemoveEmptyEntries);
        }
    }

    /// <inheritdoc/>
    /*public List<int> TokenizeSequence(List<char[]> strings)
    {
        var result = new List<int>();

        foreach (var word in strings)
        {
            var hash = CreateHash(word);

            result.Add(hash);
        }

        return result;
    }*/

    private static int CreateHash(List<char> word)
    {
        const int factor = 31;

        var hash = 0;

        var tempFactor = factor;

        foreach (var letter in word)
        {
            hash += letter * tempFactor;

            tempFactor *= factor;
        }

        return hash;
    }
}
