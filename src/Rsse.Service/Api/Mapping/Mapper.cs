using SearchEngine.Domain.ApiModels;
using SearchEngine.Domain.Dto;
using CatalogDto = SearchEngine.Domain.Dto.CatalogDto;

namespace SearchEngine.Api.Mapping;

/// <summary>
/// Расширения для маппинга
/// </summary>
public static class Mapper
{
    /// <summary>
    /// Маппинг из model в dto
    /// </summary>
    public static UpdateCredosDto MapToDto(this UpdateCredosRequest request)
    {
        var response = new UpdateCredosDto{
            NewCredos = new LoginDto{Email = request.NewCredos.Email, Password = request.NewCredos.Password},
            OldCredos = new LoginDto{Email = request.OldCredos.Email, Password = request.OldCredos.Password}
        };

        return response;
    }

    /// <summary>
    /// Маппинг из model в dto
    /// </summary>
    public static CatalogDto MapToDto(this CatalogRequest request)
    {
        var response = new CatalogDto{
            CatalogPage = request.CatalogPage,
            NotesCount = request.NotesCount,
            PageNumber = request.PageNumber,
            ErrorMessage = request.ErrorMessage,
            Direction = request.Direction
        };

        return response;
    }

    /// <summary>
    /// Маппинг из model в dto
    /// </summary>
    public static CatalogResponse MapFromDto(this CatalogDto dto)
    {
        var response = new CatalogResponse{
            CatalogPage = dto.CatalogPage,
            NotesCount = dto.NotesCount,
            PageNumber = dto.PageNumber,
            ErrorMessage = dto.ErrorMessage,
            Direction = dto.Direction
        };

        return response;
    }
}
