using System.Text;
using Rsse.Domain.Data.Contracts;
using Rsse.Domain.Data.Entities;

namespace Rsse.Tests.Common;

/// <summary>
/// Компонент поставщика однократной копии данных из файла.
/// </summary>
public sealed class FileDataOnceProvider : IDataProvider<NoteEntity>
{
    private readonly List<NoteEntity> _additionalNotes = new();

    /// <inheritdoc/>
    public async IAsyncEnumerable<NoteEntity> GetDataAsync()
    {
        Console.OutputEncoding = Encoding.UTF8;
        var notes = new List<NoteEntity>(1000);

        await using var fileStream = File.OpenRead("pg_backup_.txtnotes");
        using var reader = new StreamReader(fileStream);

        StringBuilder stringBuilder = new StringBuilder();

        while (await reader.ReadLineAsync() is { } text)
        {
            var items = text.Split('\t');

            if (items.Length != 3)
            {
                Console.WriteLine("FILE DATA PROVIDER: BROKEN CSV STRING");
                throw new Exception("Unexpected number of items");
            }

            var noteEntity = new NoteEntity
            {
                NoteId = int.Parse(items[0]),

                Title = stringBuilder.Clear().Append(items[1])
                    .Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t").ToString(),

                Text = stringBuilder.Clear().Append(items[2])
                    .Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t").ToString()
            };

            notes.Add(noteEntity);
        }

        foreach (var noteEntity in notes)
        {
            yield return noteEntity;
        }

        foreach (var noteEntity in _additionalNotes)
        {
            yield return noteEntity;
        }
    }

    public void AddNotes(List<NoteEntity> notes)
    {
        _additionalNotes.AddRange(notes);
    }
}
