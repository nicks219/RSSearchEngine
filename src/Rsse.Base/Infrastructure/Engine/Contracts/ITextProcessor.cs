namespace RandomSongSearchEngine.Infrastructure.Engine.Contracts;

public interface ITextProcessor : IDisposable
{
    public IEnumerable<Text> GetTextsFromDump(string dump);
    
    public Text ConvertStringToText(string sentence);

    public List<string> CleanUpString(string sentence);

    public List<int> GetHashSetFromStrings(IEnumerable<string> text);

    public int GetComparisionMetric(List<int> originHash, List<int> wantedHash);

    public void Setup(ConsonantChain consonantChain);
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