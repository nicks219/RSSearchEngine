using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Dto;
using SearchEngine.Service.Configuration;

namespace SearchEngine.Services;

/// <summary>
/// Функционал удаления заметок.
/// </summary>
public class DeleteService(IDataRepository repo, CatalogService catalogService, ILogger<DeleteService> logger)
{
    /// <summary>
    /// Удалить заметку.
    /// </summary>
    /// <param name="noteId">Идентификатор заметки.</param>
    /// <param name="pageNumber">Номер страницы каталога с удаляемой заметкой.</param>
    /// <returns>Актуальная страница каталога.</returns>
    public async Task<CatalogResultDto> DeleteNote(int noteId, int pageNumber)
    {
        try
        {
            await repo.DeleteNote(noteId);

            return await catalogService.ReadPage(pageNumber);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ServiceErrorMessages.DeleteNoteError);

            return new CatalogResultDto { ErrorMessage = ServiceErrorMessages.DeleteNoteError };
        }
    }
}
