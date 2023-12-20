using System;
using System.Collections.Generic;

namespace SearchEngine.Infrastructure.Tokenizer.Contracts;

public interface ITokenizerProcessor : IDisposable
{
    public List<string> PreProcessNote(string note);

    public List<int> TokenizeSequence(IEnumerable<string> strings);

    public int ComputeComparisionMetric(List<int> cachedTokens, IEnumerable<int> freshTokens);

    public void SetupChain(ConsonantChain consonantChain);

    // public IEnumerable<Text> GetTextsFromDump(string dump);

    // public Text ConvertStringToText(string sentence);
}

public class Text
{
    public readonly int Number;

    public readonly List<string> Title;

    public readonly List<string> Words;

    public Text(int number, List<string> title, List<string> text)
    {
        Number = number;

        Title = title;

        Words = text;
    }
}
