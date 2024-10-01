using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Repository.Contracts;
using static SearchEngine.Common.ModelMessages;

namespace SearchEngine.Models;

/// <summary>
/// Функционал каталога
/// </summary>
public class CatalogModel(IServiceScope serviceScope)
{
    private const int Backward = 1;
    private const int Forward = 2;
    private const int MinimalPageNumber = 1;
    private const int PageSize = 10;
    private readonly IDataRepository _repo = serviceScope.ServiceProvider.GetRequiredService<IDataRepository>();
    private readonly ILogger<CatalogModel> _logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<CatalogModel>>();

    /// <summary>
    /// Получить страницу каталога
    /// </summary>
    /// <param name="pageNumber">номер страницы</param>
    /// <returns>страница каталога</returns>
    public async Task<CatalogDto> ReadPage(int pageNumber)
    {
        try
        {
            var notesCount = await _repo.ReadNotesCount();

            var catalogPage = await _repo.ReadCatalogPage(pageNumber, PageSize).ToListAsync();

            return new CatalogDto { PageNumber = pageNumber, NotesCount = notesCount, CatalogPage = catalogPage };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ReadCatalogPageError);

            return new CatalogDto { ErrorMessage = ReadCatalogPageError };
        }
    }

    /// <summary>
    /// Перейти на другую страницу
    /// </summary>
    /// <param name="catalog">данные для навигации</param>
    /// <returns>страница каталога</returns>
    public async Task<CatalogDto> NavigateCatalog(CatalogDto catalog)
    {
        try
        {
            var direction = GetDirection(catalog.Direction);

            var pageNumber = catalog.PageNumber;

            var notesCount = await _repo.ReadNotesCount();

            pageNumber = NavigateCatalogPages(direction, pageNumber, notesCount);

            var catalogPage = await _repo.ReadCatalogPage(pageNumber, PageSize).ToListAsync();

            return new CatalogDto { PageNumber = pageNumber, NotesCount = notesCount, CatalogPage = catalogPage };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, NavigateCatalogError);

            return new CatalogDto { ErrorMessage = NavigateCatalogError };
        }
    }

    /// <summary>
    /// Удалить заметку
    /// </summary>
    /// <param name="noteId">идентификатор заметки</param>
    /// <param name="pageNumber">номер страницы каталога с удаляемой заметкой</param>
    /// <returns>актуальная страница каталога</returns>
    public async Task<CatalogDto> DeleteNote(int noteId, int pageNumber)
    {
        try
        {
            await _repo.DeleteNote(noteId);

            return await ReadPage(pageNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, DeleteNoteError);

            return new CatalogDto { ErrorMessage = DeleteNoteError };
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
