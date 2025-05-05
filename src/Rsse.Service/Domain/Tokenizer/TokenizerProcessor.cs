using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SearchEngine.Domain.Contracts;

namespace SearchEngine.Domain.Tokenizer;

/// <summary>
/// Основной функционал токенизатора
/// </summary>
public sealed class TokenizerProcessor : ITokenizerProcessor
{
    // символ для увеличения поискового веса при вычислении индекса, только для точного совпадения:
    private const string WeightExtendedChainSymbol = "@";
    private const string WordSeparatorSymbol = " ";
    private const string Numbers = "0123456789";
    private const string ReducedEnglish = "qwrtpsdfghjklzxcvbnm";
    private const string ExtendedEnglish = ReducedEnglish + /*"eyuioa" +*/ Numbers;

    private const string ReducedConsonantChain = "цкнгшщзхфвпрлджчсмтб" + ReducedEnglish; // + "яыоайуеиюэъьё"
    private const string ExtendedConsonantChain = "цкнгшщзхфвпрлджчсмтб" + "яыоайуеиюэ" + ExtendedEnglish + WeightExtendedChainSymbol;// + "ёъь"

    private string? _consonantChain;

    /// <inheritdoc/>
    public void SetupChain(ConsonantChain consonantChain)
    {
        _consonantChain = consonantChain switch
        {
            ConsonantChain.Reduced => ReducedConsonantChain,
            ConsonantChain.Extended => ExtendedConsonantChain,
            _ => throw new NotImplementedException("Unknown consonant chain")
        };
    }

    /// <inheritdoc/>
    public List<string> PreProcessNote(string note)
    {
        var stringBuilder = new StringBuilder(note.ToLower());

        // замена символов:
        stringBuilder = stringBuilder.Replace((char)13, ' '); // @"\r"
        stringBuilder = stringBuilder.Replace((char)10, ' '); // @"\n"
        stringBuilder = stringBuilder.Replace('ё', 'е');
        // деление ссылкок на части, можно сделать конфигурируемым или отключаемым:
        stringBuilder = stringBuilder.Replace(':', ' ');
        stringBuilder = stringBuilder.Replace('/', ' ');
        stringBuilder = stringBuilder.Replace('.', ' ');

        var words = stringBuilder.ToString().Split(WordSeparatorSymbol);

        return words
            .Select(word => word.Where(letter => _consonantChain!.IndexOf(letter) != -1)
                .Aggregate("", (current, value) => current + value))
                    .Where(completed => completed != "")
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

    /// <inheritdoc/>
    public int ComputeComparisionMetric(List<int> referenceTokens, IEnumerable<int> inputTokens)
    {
        return _consonantChain switch
        {
            ReducedConsonantChain => GetReducedChainMetric(referenceTokens, inputTokens),
            ExtendedConsonantChain => GetExtendedChainMetric(referenceTokens, inputTokens),
            _ => throw new NotImplementedException("Unknown compare method")
        };
    }

    /// <summary>
    /// Вычислить метрику сравнения двух векторов, для эталонного вектора на основе редуцированного набора.
    /// Последовательность токенов (т.е. "слов") не учитывается
    /// </summary>
    /// <param name="referenceTokens">эталонный вектор</param>
    /// <param name="inputTokens">сравниваемый вектор</param>
    /// <returns>метрика количества совпадений</returns>
    private static int GetReducedChainMetric(IEnumerable<int> referenceTokens, IEnumerable<int> inputTokens)
    {
        // NB "я ты он она я ты он она я ты он она" будет найдено почти во всех заметках, необходимо обработать результат

        var result = referenceTokens.Intersect(inputTokens);

        return result.Count();
    }

    /// <summary>
    /// Вычислить метрику сравнения двух векторов, для эталонного вектора на основе расширенного набора.
    /// Учитывается последовательность токенов (т.е. "слов")
    /// </summary>
    /// <param name="referenceTokens">эталонный вектор</param>
    /// <param name="inputTokens">сравниваемый вектор</param>
    /// <returns>метрика количества совпадений</returns>
    private static int GetExtendedChainMetric(List<int> referenceTokens, IEnumerable<int> inputTokens)
    {
        // NB "облака лошадки без оглядки облака лошадки без оглядки" в 227 и 270 = 5

        var result = 0;

        var startIndex = 0;

        foreach (var intersectionIndex in inputTokens.Select(value => referenceTokens.IndexOf(value, startIndex))
                     .Where(intersectionIndex => intersectionIndex != -1 && intersectionIndex >= startIndex))
        {
            result++;

            startIndex = intersectionIndex + 1;

            if (startIndex >= referenceTokens.Count)
            {
                break;
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
    }
}
