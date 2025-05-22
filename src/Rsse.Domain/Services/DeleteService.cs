using System.Threading.Tasks;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Dto;

namespace SearchEngine.Services;

/// <summary>
/// Функционал удаления заметок.
/// </summary>
public class DeleteService(IDataRepository repo, CatalogService catalogService)
{
    /// <summary>
    /// Удалить заметку.
    /// </summary>
    /// <param name="noteId">Идентификатор заметки.</param>
    /// <param name="pageNumber">Номер страницы каталога с удаляемой заметкой.</param>
    /// <returns>Актуальная страница каталога.</returns>
    public async Task<CatalogResultDto> DeleteNote(int noteId, int pageNumber)
    {
        await repo.DeleteNote(noteId);

        return await catalogService.ReadPage(pageNumber);
    }
}
