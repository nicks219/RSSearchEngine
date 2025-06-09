using System;
using System.Collections.Generic;
using SearchEngine.Service.Contracts;
using SearchEngine.Service.Tokenizer.Wrapper;

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
    public abstract int ComputeComparisionMetric(TokenVector targetVector, TokenVector searchVector);

    /// <inheritdoc/>
    public TokenVector TokenizeText(string text)
    {
        var words = text
            .ToLower()
            .Replace('ё', 'е')
            .Split(Separators, StringSplitOptions.RemoveEmptyEntries);

        var vector = new List<Token>(words.Length);

        foreach (var word in words)
        {
            var sequenceHashProcessor = new SequenceHashProcessor();
            foreach (var symbol in word)
            {
                if (ConsonantChain.Contains(symbol))
                {
                    sequenceHashProcessor.AddChar(symbol);
                }
            }

            if (sequenceHashProcessor.HasValue())
            {
                var token = sequenceHashProcessor.GetHash();
                var tokenWrapper = new Token(token);
                vector.Add(tokenWrapper);
            }
        }

        vector.TrimExcess();

        var vectorWrapper = new TokenVector(vector);
        return vectorWrapper;
    }

    /// <summary>
    /// Контейнер, вычисляющий хэш на последовательность символов по мере их добавления.
    /// </summary>
    private struct SequenceHashProcessor()
    {
        private const int Factor = 31;

        private int _hash = 0;

        private int _tempFactor = Factor;

        private bool _hasValue;

        /// <summary>
        /// Добавить символ к последовательности, по которой вычисляется хэш.
        /// </summary>
        /// <param name="letter"></param>
        public void AddChar(char letter)
        {
            _hash += letter * _tempFactor;

            _tempFactor *= Factor;

            _hasValue = true;
        }

        /// <summary>
        /// Получить текущий хэш на добавленную последовательность символов.
        /// </summary>
        /// <returns>Хэш.</returns>
        public int GetHash() => _hash;

        /// <summary>
        /// Признак наличия добавленных символов в контейнере.
        /// </summary>
        /// <returns><b>true</b> - символы были добавлены.</returns>
        public bool HasValue() => _hasValue;
    }
}
