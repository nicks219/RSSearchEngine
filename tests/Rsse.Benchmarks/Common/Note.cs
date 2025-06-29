namespace RsseEngine.Benchmarks.Common;

public class Note(int noteId, string title, string text)
{
    public int NoteId = noteId;
    public string Title = title.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t");
    public string Text = text.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t");
}
