using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Dto;
using static SearchEngine.Domain.Configuration.ErrorMessages;

namespace SearchEngine.Domain.Managers;

/// <summary>
/// Функционал каталога
/// </summary>
public class CatalogManager(IDataRepository repo, ILogger<CatalogManager> logger)
{
    public const int Backward = 1;
    public const int Forward = 2;
    private const int MinimalPageNumber = 1;
    private const int PageSize = 10;

    /// <summary>
    /// Получить страницу каталога
    /// </summary>
    /// <param name="pageNumber">номер страницы</param>
    /// <returns>страница каталога</returns>
    public async Task<CatalogResultDto> ReadPage(int pageNumber)
    {
        try
        {
            var notesCount = await repo.ReadNotesCount();

            var catalogPage = await repo.ReadCatalogPage(pageNumber, PageSize);

            return new CatalogResultDto { PageNumber = pageNumber, NotesCount = notesCount, CatalogPage = catalogPage };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ReadCatalogPageError);

            return new CatalogResultDto { ErrorMessage = ReadCatalogPageError };
        }
    }

    /// <summary>
    /// Перейти на другую страницу
    /// </summary>
    /// <param name="catalogRequest">данные для навигации</param>
    /// <returns>страница каталога</returns>
    public async Task<CatalogResultDto> NavigateCatalog(CatalogRequestDto catalogRequest)
    {
        try
        {
            var direction = GetDirection(catalogRequest.Direction);

            var pageNumber = catalogRequest.PageNumber;

            var notesCount = await repo.ReadNotesCount();

            pageNumber = NavigateCatalogPages(direction, pageNumber, notesCount);

            var catalogPage = await repo.ReadCatalogPage(pageNumber, PageSize);

            return new CatalogResultDto { PageNumber = pageNumber, NotesCount = notesCount, CatalogPage = catalogPage };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, NavigateCatalogError);

            return new CatalogResultDto { ErrorMessage = NavigateCatalogError };
        }
    }

    /// <summary>
    /// Удалить заметку
    /// </summary>
    /// <param name="noteId">идентификатор заметки</param>
    /// <param name="pageNumber">номер страницы каталога с удаляемой заметкой</param>
    /// <returns>актуальная страница каталога</returns>
    public async Task<CatalogResultDto> DeleteNote(int noteId, int pageNumber)
    {
        try
        {
            await repo.DeleteNote(noteId);

            return await ReadPage(pageNumber);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, DeleteNoteError);

            return new CatalogResultDto { ErrorMessage = DeleteNoteError };
        }
    }

    /// <summary>
    /// Получить направление перемещения по каталогу в виде константы
    /// </summary>
    private static int GetDirection(List<int>? direction)
    {
        if (direction is null)
        {
            return 0;
        }

        return direction[0] switch
        {
            Backward => Backward,
            Forward => Forward,
            _ => throw new NotImplementedException($"[{nameof(GetDirection)}] unknown direction")
        };
    }

    private static int NavigateCatalogPages(int direction, int pageNumber, int notesCount)
    {
        switch (direction)
        {
            case Forward:
                {
                    var pageCount = Math.DivRem(notesCount, PageSize, out var remainder);

                    if (remainder > 0)
                    {
                        pageCount++;
                    }

                    if (pageNumber < pageCount)
                    {
                        pageNumber++;
                    }

                    break;
                }
            case Backward:
                {
                    if (pageNumber > MinimalPageNumber) pageNumber--;
                    break;
                }
        }

        if (pageNumber < MinimalPageNumber)
        {
            pageNumber = MinimalPageNumber;
        }

        return pageNumber;
    }
}
