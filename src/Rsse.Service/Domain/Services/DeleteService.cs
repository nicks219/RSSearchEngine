using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SearchEngine.Domain.Configuration;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Dto;

namespace SearchEngine.Domain.Services;

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
            logger.LogError(ex, ErrorMessages.DeleteNoteError);

            return new CatalogResultDto { ErrorMessage = ErrorMessages.DeleteNoteError };
        }
    }
}
