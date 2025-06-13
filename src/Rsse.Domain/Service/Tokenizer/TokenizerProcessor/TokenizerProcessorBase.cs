using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using SearchEngine.Service.Tokenizer.Contracts;
using SearchEngine.Service.Tokenizer.Dto;

namespace SearchEngine.Service.Tokenizer.TokenizerProcessor;

/// <summary>
/// Базовый функционал токенизатора.
/// </summary>
public abstract class TokenizerProcessorBase : ITokenizerProcessor
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
    public abstract int ComputeComparisonScore(TokenVector targetVector, TokenVector searchVector, int searchStartIndex = 0);

    /// <inheritdoc/>
    public TokenVector TokenizeText(string text)
    {
        var vector = TokenizeTextInternal(text);
        return vector;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TokenVector TokenizeTextInternal(string text)
    {
        var count = text.Count(e => e == ' ') + 1;

        var tokens = new List<int>(count);
        var sequenceHashProcessor = new SequenceHashProcessor();

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

        tokens.TrimExcess();

        var resultVector = new TokenVector(tokens);
        return resultVector;
    }

    /// <inheritdoc/>
    [Obsolete("Используйте TokenizeTextInternal(string text)")]
    public TokenVector TokenizeText(string[] words)
    {
        var vector = TokenizeTextInternal(words);
        return vector;
    }

    /// <summary>
    /// Получить обработанный и разбитый на слова текст.
    /// </summary>
    /// <param name="text">Текст в формате строки.</param>
    /// <returns>Обработанный и разбитый на слова текст.</returns>
    [Obsolete("Используйте TokenizeTextInternal(string text)")]
    private static string[] PreProcessTest(string text)
    {
        var words = text
            .ToLower()
            .Replace('ё', 'е')
            .Split(Separators, StringSplitOptions.RemoveEmptyEntries);

        return words;
    }

    [Obsolete("Используйте TokenizeTextInternal(string text)")]
    private TokenVector TokenizeTextInternal(string[] words)
    {
        var tokens = new List<int>(words.Length);

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
                var hash = sequenceHashProcessor.GetHashAndReset();
                tokens.Add(hash);
            }
        }

        tokens.TrimExcess();

        var resultVector = new TokenVector(tokens);
        return resultVector;
    }
}
