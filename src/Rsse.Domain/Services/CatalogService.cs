using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Dto;

namespace SearchEngine.Services;

/// <summary>
/// Функционал каталога.
/// </summary>
public class CatalogService(IDataRepository repo)
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
    /// Получить страницу каталога по номеру.
    /// </summary>
    /// <param name="pageNumber">Номер страницы.</param>
    /// <returns>Страница каталога.</returns>
    public async Task<CatalogResultDto> ReadPage(int pageNumber)
    {
        var notesCount = await repo.ReadNotesCount();

        var catalogPage = await repo.ReadCatalogPage(pageNumber, PageSize);

        return new CatalogResultDto { PageNumber = pageNumber, NotesCount = notesCount, CatalogPage = catalogPage };
    }

    /// <summary>
    /// Перейти на другую страницу.
    /// </summary>
    /// <param name="catalogRequest">Данные для навигации.</param>
    /// <returns>Страница каталога.</returns>
    public async Task<CatalogResultDto> NavigateCatalog(CatalogRequestDto catalogRequest)
    {
        var direction = GetDirection(catalogRequest.Direction);

        var pageNumber = catalogRequest.PageNumber;

        var notesCount = await repo.ReadNotesCount();

        pageNumber = NavigateCatalogPages(direction, pageNumber, notesCount);

        var catalogPage = await repo.ReadCatalogPage(pageNumber, PageSize);

        return new CatalogResultDto { PageNumber = pageNumber, NotesCount = notesCount, CatalogPage = catalogPage };
    }

    /// <summary>
    /// Получить направление перемещения по каталогу в виде константы.
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
            _ => throw new NotSupportedException($"[{nameof(GetDirection)}] unknown direction")
        };
    }

    /// <summary>
    /// Получить номер страницы каталога после навигации.
    /// </summary>
    /// <param name="direction">Направление перехода.</param>
    /// <param name="pageNumber">Текущая страница.</param>
    /// <param name="notesCount">Количсемтво заметок на странице.</param>
    /// <returns></returns>
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
