using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SearchEngine.Infrastructure.Engine.Contracts;

namespace SearchEngine.Infrastructure.Tokenizer;

public enum ConsonantChain
{
    Undefined = 0,
    Defined = 1
}
public sealed class TokenizerProcessor : ITokenizerProcessor
{
    private const string Specials = "@";
    private const string Numbers = "0123456789";
    private const string UndefinedEnglish = "qwrtpsdfghjklzxcvbnm";
    private const string DefinedEnglish = UndefinedEnglish + /*"eyuioa" +*/ Numbers;

    private const string UndefinedChainConsonant = "цкнгшщзхфвпрлджчсмтб" + UndefinedEnglish; // + "яыоайуеиюэъьё"
    private const string DefinedChainConsonant = "цкнгшщзхфвпрлджчсмтб" + "яыоайуеиюэ" + DefinedEnglish + Specials;// + "ёъь"

    // [TODO] убери ограничение на 10 результатов, ну или 15..20 хоть сделай
    // [TODO] тэг у меня сейчас @ - решетка # не взлеьела из-за фронта =)

    // private static readonly Regex SongPattern = new(@"\(\d+,'[^']+','[^']+'\)", RegexOptions.Compiled);
    // private static readonly Regex NumberPattern = new(@"\d+", RegexOptions.Compiled);
    // private static readonly Regex TitlePattern = new(@"'[^']+'", RegexOptions.Compiled);
    private string? _consonantChain;

    public void Setup(ConsonantChain consonantChain)
    {
        _consonantChain = consonantChain switch
        {
            ConsonantChain.Undefined => UndefinedChainConsonant,
            ConsonantChain.Defined => DefinedChainConsonant,
            _ => throw new NotImplementedException("Unknown consonant chain")
        };
    }

    public List<string> CleanUpString(string sentence)
    {
        var stringBuilder = new StringBuilder(sentence.ToLower());

        // replaces
        stringBuilder = stringBuilder.Replace((char)13, ' '); // @"\r"
        stringBuilder = stringBuilder.Replace((char)10, ' '); // @"\n"
        stringBuilder = stringBuilder.Replace('ё', 'е');
        // делим ссылки (а стоит ли?)
        stringBuilder = stringBuilder.Replace(':', ' ');
        stringBuilder = stringBuilder.Replace('/', ' ');
        stringBuilder = stringBuilder.Replace('.', ' ');

        var words = stringBuilder.ToString().Split(" ");

        var res = new List<string>();

        // var res = words.Select(word => word.Where(letter => _consonant.IndexOf(letter) != -1).Aggregate("", (current, letter) => current + letter)).Where(currentWord => currentWord != "").ToList();

        foreach (var word in words)
        {
            var currentWord = "";

            foreach (var letter in word)
            {
                if (_consonantChain!.IndexOf(letter) != -1)
                {
                    currentWord += letter;
                }
            }

            if (currentWord != "")
            {
                res.Add(currentWord);
            }
        }

        return res;
    }

    public List<int> GetHashSetFromStrings(IEnumerable<string> text)
    {
        const int factor = 31;

        var result = new List<int>();

        foreach (var word in text)
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

    public int GetComparisionMetric(List<int> originHash, List<int> wantedHash)
    {
        return _consonantChain switch
        {
            UndefinedChainConsonant => GetUndefinedChainMetric(originHash, wantedHash),
            DefinedChainConsonant => GetDefinedChainMetric(originHash, wantedHash),
            _ => throw new NotImplementedException("Unknown compare method")
        };
    }

    private static int GetUndefinedChainMetric(IEnumerable<int> baseHash, IEnumerable<int> searchHash)
    {
        // нечеткий поиск без последовательности
        // 'я ты он она я ты он она я ты он она' будет найдено почти во всех песнях
        // можно "добавлять баллы" за правильную последовательность
        // var r = GetDefinedChainMetric(baseHash.ToList(), searchHash.ToList()); // 171

        var res = baseHash.Intersect(searchHash);

        return res.Count();
    }

    private static int GetDefinedChainMetric(List<int> baseHash, List<int> searchHash)
    {
        // четкий поиск с последовательностью
        // `облака лошадки без оглядки облака лошадки без оглядки` в 227 и 270 = 5

        var res = 0;

        var startIndex = 0;

        // foreach (var intersectionIndex in searchHash.Select(hash => baseHash.IndexOf(hash)).Where(intersectionIndex => intersectionIndex != -1 && intersectionIndex >= startIndex))

        foreach (var hash in searchHash)
        {
            var intersectionIndex = baseHash.IndexOf(hash, startIndex);

            if (intersectionIndex == -1 || intersectionIndex < startIndex) continue;

            res++;

            startIndex = ++intersectionIndex;

            if (startIndex >= baseHash.Count)
            {
                break;
            }
        }

        return res;
    }

    public void Dispose()
    {
    }
}
