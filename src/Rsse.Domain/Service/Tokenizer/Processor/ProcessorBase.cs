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
    public List<int> TokenizeNote(string note)
    {
        var words = PreProcess(note);
        var preProcessedWord = new List<char>();
        var vector = new List<int>();

        foreach (var word in words)
        {
            foreach (var symbol in word)
            {
                if (ConsonantChain.Contains(symbol))
                {
                    preProcessedWord.Add(symbol);
                }
            }

            if (preProcessedWord.Count > 0)
            {
                var hash = CreateHash(preProcessedWord);
                vector.Add(hash);
                preProcessedWord.Clear();
            }
        }

        return vector;

        string[] PreProcess(string text)
        {
            var preProcessedText = text
                .ToLower()
                .Replace('ё', 'е')
                .Split(Separators, StringSplitOptions.RemoveEmptyEntries);

            return preProcessedText;
        }
    }

    private static int CreateHash(List<char> symbols)
    {
        const int factor = 31;

        var hash = 0;

        var tempFactor = factor;

        foreach (var symbol in symbols)
        {
            hash += symbol * tempFactor;

            tempFactor *= factor;
        }

        return hash;
    }
}

/*
// вариант с переносом алгоритма в класс.
/// <inheritdoc/>
    public List<int> TokenizeNote(string note)
    {
        var vector = new List<int>();
        foreach (var word in Split())
        {
            var hashProcessor = new HashProcessor();

            foreach (var symbol in word)
            {
                if (ConsonantChain.Contains(symbol))
                {
                    hashProcessor.AddSymbol(symbol);
                }
            }

            if (hashProcessor.HasValue())
            {
                var hash = hashProcessor.GetHash();
                vector.Add(hash);
            }
        }

        return vector;

        string[] Split()
        {
            return note
                .ToLower()
                .Replace('ё', 'е')
                .Split(Separators, StringSplitOptions.RemoveEmptyEntries);
        }
    }

    private struct HashProcessor()
    {
        private const int Factor = 31;

        private int _hash;

        private int _tempFactor = Factor;

        private bool _hasValue;

        public int GetHash() => _hash;

        public void AddSymbol(char symbol)
        {
            _hash += symbol * _tempFactor;
            _tempFactor *= Factor;
            _hasValue = true;
        }

        public bool HasValue() => _hasValue;
    }
}*/
