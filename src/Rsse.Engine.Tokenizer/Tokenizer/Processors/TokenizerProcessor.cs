using System.Collections.Generic;
using System.Linq;
using RsseEngine.Dto;
using RsseEngine.Tokenizer.Contracts;

namespace RsseEngine.Tokenizer.Processors;

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
    /// <param name="words">Текст в виде массива строк.</param>
    /// <returns>Результат в виде вектора.</returns>
    public TokenVector TokenizeText(params string[] words)
    {
        // Вызывается при инициализации индекса.
        var vector = TokenizeTextInternal(words);
        return vector;
    }

    /// <summary>
    /// Токенизировать текст в виде строки.
    /// </summary>
    /// <param name="words">Текст в виде строки.</param>
    /// <returns>Результат в виде вектора.</returns>
    public TokenVector TokenizeText(string words)
    {
        // Вызывается на поисковых запросах.
        var vector = TokenizeTextInternal(words);
        return vector;
    }

    // Токенизировать текст.
    private TokenVector TokenizeTextInternal(params string[] words)
    {
        var count = words[0].Count(e => e == ' ') + 1;

        var tokens = new List<int>(count);
        var sequenceHashProcessor = new SequenceHashProcessor();

        foreach (var text in words)
        {
            for (var index = 0; index < text.Length; index++)
            {
                var symbol = char.ToLower(text[index]);
                if (symbol == 'ё') symbol = 'е';

                if (Separators.Contains(symbol))
                {
                    if (sequenceHashProcessor.HasValue())
                    {
                        var hash = sequenceHashProcessor.GetHashAndReset();
                        tokens.Add(hash);
                    }

                    continue;
                }

                if (ConsonantChain.Contains(symbol))
                {
                    sequenceHashProcessor.AddChar(symbol);
                }
            }

            if (sequenceHashProcessor.HasValue())
            {
                var hash = sequenceHashProcessor.GetHashAndReset();
                tokens.Add(hash);
            }
        }

        tokens.TrimExcess();

        var resultVector = new TokenVector(tokens);
        return resultVector;
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
