using System.Threading;
using System.Threading.Tasks;
using SearchEngine.Data.Contracts;

namespace SearchEngine.Services;

/// <summary>
/// Функционал удаления заметок.
/// </summary>
public class DeleteService(IDataRepository repo)
{
    /// <summary>
    /// Удалить заметку.
    /// </summary>
    /// <param name="noteId">Идентификатор заметки.</param>
    /// <param name="ct">Токен отмены.</param>
    public async Task DeleteNote(int noteId, CancellationToken ct)
    {
        await repo.DeleteNote(noteId, ct);
    }
}
