using System.Collections.Generic;
using System.Linq;
using SearchEngine.Tokenizer.Contracts;
using SearchEngine.Tokenizer.Dto;

namespace SearchEngine.Tokenizer.TokenizerProcessor;

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
    public TokenVector TokenizeText(string[] words)
    {
        // Вызывается при инициализации индекса.
        var vector = TokenizeTextInternal(words);
        return vector;
    }

    /// <inheritdoc/>
    public TokenVector TokenizeText(string words)
    {
        // Вызывается на поисковых запросах.
        var vector = TokenizeTextInternal(words);
        return vector;
    }

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
}
