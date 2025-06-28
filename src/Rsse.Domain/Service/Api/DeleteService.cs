using System.Threading;
using System.Threading.Tasks;
using Rsse.Domain.Data.Contracts;

namespace Rsse.Domain.Service.Api;

/// <summary>
/// Функционал удаления заметок.
/// </summary>
public class DeleteService(IDataRepository repo)
{
    /// <summary>
    /// Удалить заметку.
    /// </summary>
    /// <param name="noteId">Идентификатор заметки.</param>
    /// <param name="stoppingToken">Токен отмены.</param>
    public async Task DeleteNote(int noteId, CancellationToken stoppingToken)
    {
        await repo.DeleteNote(noteId, stoppingToken);
    }
}
