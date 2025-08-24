using System;
using System.Collections.Generic;
using System.Globalization;
using RsseEngine.Tokenizer.Contracts;

namespace RsseEngine.Tokenizer.Processor;

/// <summary>
/// Базовый функционал токенизатора текста.
/// </summary>
public abstract class TokenizerProcessor
{
    // Сокращенный набор символов из английского алфавита.
    private const string ReducedEnglish = "qwrtpsdfghjklzxcvbnm";

    // Разделители слов в заметке.
    private static readonly char[] Separators = ['\r', '\n', '\t', ':', '/', '.', ' '];

    // Архитектурное ограничение.
    private TokenizerProcessor() { }

    /// <summary>
    /// Полный набор символов для токенизации.
    /// </summary>
    protected abstract string ConsonantChain { get; }

    /// <summary>
    /// Токенизировать текст в виде массива строк.
    /// </summary>
    /// <param name="tokens">Токенизированый текст.</param>
    /// <param name="text">Текст в виде массива строк.</param>
    public void TokenizeText(List<int> tokens, params Span<string> text)
    {
        const int bufferSize = 128;

        Span<char> buffer = stackalloc char[bufferSize];

        var separators = Separators.AsSpan();
        var consonantChain = ConsonantChain.AsSpan();

        var sequenceHashProcessor = new SequenceHashProcessor();

        foreach (var word in text)
        {
            var wordAsSpan = word.AsSpan();

            for (var i = 0; i < word.Length; i += bufferSize)
            {
                var count = wordAsSpan.Slice(i, Math.Min(bufferSize, word.Length - i))
                    .ToLower(buffer, CultureInfo.CurrentCulture);

                for (var index = 0; index < count; index++)
                {
                    var symbol = buffer[index];
                    if (symbol == 'ё') symbol = 'е';

                    if (separators.Contains(symbol))
                    {
                        if (sequenceHashProcessor.HasValue())
                        {
                            var hash = sequenceHashProcessor.GetHashAndReset();
                            tokens.Add(hash);
                        }

                        continue;
                    }

                    if (consonantChain.Contains(symbol))
                    {
                        sequenceHashProcessor.AddChar(symbol);
                    }
                }
            }

            if (sequenceHashProcessor.HasValue())
            {
                var hash = sequenceHashProcessor.GetHashAndReset();
                tokens.Add(hash);
            }
        }
    }

    /// <summary>
    /// Токенизатор, использующий расширенный набор символов.
    /// </summary>
    public sealed class Extended : TokenizerProcessor, ITokenizerProcessor
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
    }

    /// <summary>
    /// Токенизатор, использующий урезанный набор символов.
    /// </summary>
    public sealed class Reduced : TokenizerProcessor, ITokenizerProcessor
    {
        // полностью сформированный сокращенный набор символов для токенизации, может включать: "яыоайуеиюэъьё".
        private const string ReducedConsonantChain = "цкнгшщзхфвпрлджчсмтб" + ReducedEnglish;

        /// <inheritdoc/>
        protected override string ConsonantChain => ReducedConsonantChain;
    }
}
