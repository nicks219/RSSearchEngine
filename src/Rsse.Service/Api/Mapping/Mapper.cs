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
    public static UpdateCredosDto MapToDto(this UpdateCredentialsRequest request)
    {
        var response = new UpdateCredosDto
        {
            NewCredos = new CredentialsDto { Email = request.NewCredos.Email, Password = request.NewCredos.Password },
            OldCredos = new CredentialsDto { Email = request.OldCredos.Email, Password = request.OldCredos.Password }
        };

        return response;
    }

    /// <summary>
    /// Маппинг из model в dto
    /// </summary>
    public static CatalogDto MapToDto(this CatalogRequest request)
    {
        var response = new CatalogDto
        {
            PageNumber = request.PageNumber,
            Direction = request.Direction
        };

        return response;
    }

    /// <summary>
    /// Маппинг из dto в model
    /// </summary>
    public static CatalogResponse MapFromDto(this CatalogDto dto)
    {
        var response = new CatalogResponse
        {
            CatalogPage = dto.CatalogPage,
            NotesCount = dto.NotesCount,
            PageNumber = dto.PageNumber,
            ErrorMessage = dto.ErrorMessage
        };

        return response;
    }

    /// <summary>
    /// Маппинг из dto в model
    /// </summary>
    public static NoteDto MapToDto(this NoteRequest request)
    {
        var response = new NoteDto
        {
            TagsCheckedRequest = request.TagsCheckedRequest,
            TitleRequest = request.TitleRequest,
            TextRequest = request.TextRequest,
            NoteIdExchange = request.NoteIdExchange,
        };

        return response;
    }

    /// <summary>
    /// Маппинг из dto в model
    /// </summary>
    public static NoteResponse MapFromDto(this NoteDto dto)
    {
        var response = new NoteResponse
        {
            TagsCheckedUncheckedResponse = dto.TagsCheckedUncheckedResponse,
            TitleResponse = dto.TitleResponse,
            TextResponse = dto.TextResponse,
            StructuredTagsListResponse = dto.StructuredTagsListResponse,
            NoteIdExchange = dto.NoteIdExchange,
            CommonErrorMessageResponse = dto.CommonErrorMessageResponse,
        };

        return response;
    }
}
