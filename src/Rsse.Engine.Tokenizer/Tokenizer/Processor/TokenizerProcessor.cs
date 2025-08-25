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
    /// Токенизировать текст из массива строк.
    /// </summary>
    /// <param name="textTokens">Результат токенизации текста.</param>
    /// <param name="text">Текст в виде массива строк.</param>
    public void TokenizeText(List<int> textTokens, params Span<string> text)
    {
        const int bufferSize = 128;

        Span<char> buffer = stackalloc char[bufferSize];

        ReadOnlySpan<char> separators = Separators.AsSpan();
        var consonantChain = ConsonantChain.AsSpan();

        var sequenceHashProcessor = new SequenceHashProcessor();

        foreach (var textPart in text)
        {
            var textPartAsSpan = textPart.AsSpan();

            for (var sliceIndex = 0; sliceIndex < textPart.Length; sliceIndex += bufferSize)
            {
                // конвертируем в нижний регистр сразу кусок текста
                var bufferLength = textPartAsSpan
                    .Slice(sliceIndex, Math.Min(bufferSize, textPart.Length - sliceIndex))
                    .ToLower(buffer, CultureInfo.CurrentCulture);

                // токенизируем слово
                for (var bufferIndex = 0; bufferIndex < bufferLength; bufferIndex++)
                {
                    var symbol = buffer[bufferIndex];
                    if (symbol == 'ё') symbol = 'е';

                    if (consonantChain.Contains(symbol))
                    {
                        sequenceHashProcessor.AddChar(symbol);
                        continue;
                    }

                    if (separators.Contains(symbol) && sequenceHashProcessor.HasValue())
                    {
                        var hash = sequenceHashProcessor.GetHashAndReset();
                        textTokens.Add(hash);
                    }
                }
            }

            if (sequenceHashProcessor.HasValue())
            {
                var hash = sequenceHashProcessor.GetHashAndReset();
                textTokens.Add(hash);
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
