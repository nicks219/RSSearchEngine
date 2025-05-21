using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SearchEngine.Domain.Contracts;
using SearchEngine.Domain.Dto;
using static SearchEngine.Domain.Configuration.ServiceErrorMessages;

namespace SearchEngine.Domain.Services;

/// <summary>
/// Функционал каталога
/// </summary>
public class CatalogService(IDataRepository repo, ILogger<CatalogService> logger)
{
    /// <summary/> Направление навигации по каталогу.
    public enum Direction
    {
        Backward = 1,
        Forward = 2
    }

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
    /// Получить направление перемещения по каталогу в виде константы
    /// </summary>
    private static Direction GetDirection(List<int>? direction)
    {
        if (direction is null)
        {
            return 0;
        }

        var current = (Direction)direction.ElementAt(0);
        return current switch
        {
            Direction.Backward => Direction.Backward,
            Direction.Forward => Direction.Forward,
            _ => throw new NotImplementedException($"[{nameof(GetDirection)}] unknown direction")
        };
    }

    private static int NavigateCatalogPages(Direction direction, int pageNumber, int notesCount)
    {
        switch (direction)
        {
            case Direction.Forward:
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
            case Direction.Backward:
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
