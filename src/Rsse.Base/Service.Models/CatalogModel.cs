using Microsoft.EntityFrameworkCore;
using RandomSongSearchEngine.Data.DTO;
using RandomSongSearchEngine.Data.Repository.Contracts;

namespace RandomSongSearchEngine.Service.Models;

public class CatalogModel
{
    #region Defaults

    private const int Forward = 2;
    private const int Backward = 1;
    private const int MinimalPageNumber = 1;
    private const int PageSize = 10;
    private readonly IDataRepository _repo;
    private readonly ILogger<CatalogModel> _logger;

    #endregion

    public CatalogModel(IServiceScope serviceScope)
    {
        _repo = serviceScope.ServiceProvider.GetRequiredService<IDataRepository>();
        
        _logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<CatalogModel>>();
    }

    public async Task<CatalogDto> ReadCatalogPage(int pageNumber)
    {
        try
        {
            var notesCount = await _repo.ReadNotesCount();
            
            var catalogPage = await _repo.ReadCatalogPage(pageNumber, PageSize).ToListAsync();
            
            return CreateCatalogDto(pageNumber, notesCount, catalogPage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[{nameof(CatalogModel)}: {nameof(ReadCatalogPage)} error]");
            
            return new CatalogDto { ErrorMessage = $"[{nameof(CatalogModel)}: {nameof(ReadCatalogPage)} error]" };
        }
    }

    public async Task<CatalogDto> NavigateCatalog(CatalogDto catalog)
    {
        try
        {
            var direction = catalog.Direction();
            
            var pageNumber = catalog.PageNumber;
            
            var notesCount = await _repo.ReadNotesCount();
            
            pageNumber = NavigateCatalogPages(direction, pageNumber, notesCount);
            
            var catalogPage = await _repo.ReadCatalogPage(pageNumber, PageSize).ToListAsync();
            
            return CreateCatalogDto(pageNumber, notesCount, catalogPage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[{nameof(CatalogModel)}: {nameof(NavigateCatalog)} error]");
            
            return new CatalogDto { ErrorMessage = $"[{nameof(CatalogModel)}: {nameof(NavigateCatalog)} error]" };
        }
    }

    public async Task<CatalogDto> DeleteNote(int noteId, int pageNumber)
    {
        try
        {
            await _repo.DeleteNote(noteId);
            
            return await ReadCatalogPage(pageNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[{nameof(CatalogModel)}: {nameof(DeleteNote)} error]");
            
            return new CatalogDto { ErrorMessage = $"[{nameof(CatalogModel)}: {nameof(DeleteNote)} error]" };
        }
    }

    private static int NavigateCatalogPages(int navigation, int pageNumber, int songsCount)
    {
        switch (navigation)
        {
            case Forward:
            {
                var pageCount = Math.DivRem(songsCount, PageSize, out var remainder);
            
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

        return pageNumber;
    }

    private static CatalogDto CreateCatalogDto(int pageNumber, int songsCount, List<Tuple<string, int>>? catalogPage)
    {
        return new CatalogDto
        {
            PageNumber = pageNumber,
            CatalogPage = catalogPage ?? new List<Tuple<string, int>>(),
            SongsCount = songsCount
        };
    }
}