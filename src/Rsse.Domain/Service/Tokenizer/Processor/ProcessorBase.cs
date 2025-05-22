using System.Collections.Generic;
using System.Linq;
using System.Text;
using SearchEngine.Service.Contracts;

namespace SearchEngine.Service.Tokenizer.Processor;

/// <summary>
/// Базовый функционал токенизатора.
/// </summary>
public abstract class ProcessorBase : ITokenizerProcessor
{
    // сокращенный набор символов из английского алфавита.
    protected const string ReducedEnglish = "qwrtpsdfghjklzxcvbnm";

    // разделитель слов в заметке.
    private const string WordSeparatorSymbol = " ";

    /// <summary>
    /// Полный набор символов для токенизации.
    /// </summary>
    protected abstract string ConsonantChain { get; }

    /// <inheritdoc/>
    public abstract int ComputeComparisionMetric(List<int> referenceTokens, IEnumerable<int> inputTokens);

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
            .Select(word => word.Where(letter => ConsonantChain.Contains(letter))
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
}
