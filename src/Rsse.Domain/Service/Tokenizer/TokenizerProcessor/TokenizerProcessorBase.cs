using System;
using System.Collections.Generic;
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
    public abstract int ComputeComparisonScore(TokenVector targetVector, TokenVector searchVector);

    /// <inheritdoc/>
    public TokenVector TokenizeText(string text)
    {
        var words = text
            .ToLower()
            .Replace('ё', 'е')
            .Split(Separators, StringSplitOptions.RemoveEmptyEntries);

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
                var hash = sequenceHashProcessor.GetHash();
                tokens.Add(hash);
            }
        }

        tokens.TrimExcess();

        var resultVector = new TokenVector(tokens);
        return resultVector;
    }
}
