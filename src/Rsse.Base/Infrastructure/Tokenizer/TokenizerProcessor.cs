using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SearchEngine.Infrastructure.Tokenizer.Contracts;

namespace SearchEngine.Infrastructure.Tokenizer;

public enum ConsonantChain
{
    Reduced = 0,
    Extended = 1
}
public sealed class TokenizerProcessor : ITokenizerProcessor
{
    private const string Specials = "@";
    private const string Numbers = "0123456789";
    private const string ReducedEnglish = "qwrtpsdfghjklzxcvbnm";
    private const string ExtendedEnglish = ReducedEnglish + /*"eyuioa" +*/ Numbers;

    private const string ReducedConsonantChain = "цкнгшщзхфвпрлджчсмтб" + ReducedEnglish; // + "яыоайуеиюэъьё"
    private const string ExtendedConsonantChain = "цкнгшщзхфвпрлджчсмтб" + "яыоайуеиюэ" + ExtendedEnglish + Specials;// + "ёъь"

    // [TODO] убери ограничение на 10 результатов, ну или 15..20 хоть сделай
    // [TODO] тэг у меня сейчас @ - решетка # не взлетела из-за ограничений фронта =)

    // private static readonly Regex SongPattern = new(@"\(\d+,'[^']+','[^']+'\)", RegexOptions.Compiled);
    // private static readonly Regex NumberPattern = new(@"\d+", RegexOptions.Compiled);
    // private static readonly Regex TitlePattern = new(@"'[^']+'", RegexOptions.Compiled);
    private string? _consonantChain;

    public void SetupChain(ConsonantChain consonantChain)
    {
        _consonantChain = consonantChain switch
        {
            ConsonantChain.Reduced => ReducedConsonantChain,
            ConsonantChain.Extended => ExtendedConsonantChain,
            _ => throw new NotImplementedException("Unknown consonant chain")
        };
    }

    public List<string> PreProcessNote(string note)
    {
        var stringBuilder = new StringBuilder(note.ToLower());

        // заменяем символы
        stringBuilder = stringBuilder.Replace((char)13, ' '); // @"\r"
        stringBuilder = stringBuilder.Replace((char)10, ' '); // @"\n"
        stringBuilder = stringBuilder.Replace('ё', 'е');
        // делим ссылки (точно необходимо?)
        stringBuilder = stringBuilder.Replace(':', ' ');
        stringBuilder = stringBuilder.Replace('/', ' ');
        stringBuilder = stringBuilder.Replace('.', ' ');

        var words = stringBuilder.ToString().Split(" ");

        return words
            .Select(word => word.Where(letter => _consonantChain!.IndexOf(letter) != -1)
                .Aggregate("", (current, value) => current + value))
                    .Where(completed => completed != "")
                        .ToList();
    }

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

    public int ComputeComparisionMetric(List<int> cachedTokens, IEnumerable<int> newTokens)
    {
        return _consonantChain switch
        {
            ReducedConsonantChain => GetReducedChainMetric(cachedTokens, newTokens),
            ExtendedConsonantChain => GetExtendedChainMetric(cachedTokens, newTokens),
            _ => throw new NotImplementedException("Unknown compare method")
        };
    }

    private static int GetReducedChainMetric(IEnumerable<int> cachedTokens, IEnumerable<int> freshTokens)
    {
        // нечеткий поиск без последовательности
        // 'я ты он она я ты он она я ты он она' будет найдено почти во всех песнях
        // можно "добавлять баллы" за правильную последовательность
        // var r = GetDefinedChainMetric(baseHash.ToList(), searchHash.ToList()); // 171

        var result = cachedTokens.Intersect(freshTokens);

        return result.Count();
    }

    private static int GetExtendedChainMetric(List<int> cachedTokens, IEnumerable<int> freshTokens)
    {
        // четкий поиск с последовательностью
        // `облака лошадки без оглядки облака лошадки без оглядки` в 227 и 270 = 5

        var result = 0;

        var startIndex = 0;

        foreach (var intersectionIndex in freshTokens.Select(value => cachedTokens.IndexOf(value, startIndex))
                     .Where(intersectionIndex => intersectionIndex != -1 && intersectionIndex >= startIndex))
        {
            result++;

            startIndex = intersectionIndex + 1;

            if (startIndex >= cachedTokens.Count)
            {
                break;
            }
        }

        return result;
    }

    public void Dispose()
    {
    }
}
