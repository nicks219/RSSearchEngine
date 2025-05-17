using SearchEngine.Domain.ApiModels;
using SearchEngine.Domain.Dto;

namespace SearchEngine.Api.Mapping;

/// <summary>
/// Расширения для маппинга
/// </summary>
public static class Mapper
{
    /// <summary>
    /// Маппинг из request model в request dto
    /// </summary>
    public static UpdateCredosRequestDto MapToDto(this UpdateCredentialsRequest request)
    {
        var response = new UpdateCredosRequestDto
        {
            NewCredos = new CredentialsRequestDto { Email = request.NewCredos.Email, Password = request.NewCredos.Password },
            OldCredos = new CredentialsRequestDto { Email = request.OldCredos.Email, Password = request.OldCredos.Password }
        };

        return response;
    }

    /// <summary>
    /// Маппинг из request model в request dto
    /// </summary>
    public static CatalogRequestDto MapToDto(this CatalogRequest request)
    {
        var response = new CatalogRequestDto
        {
            PageNumber = request.PageNumber,
            Direction = request.Direction
        };

        return response;
    }

    /// <summary>
    /// Маппинг из result dto в response model
    /// </summary>
    public static CatalogResponse MapFromDto(this CatalogResultDto dto)
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
    /// Маппинг из request dto в request model
    /// </summary>
    public static NoteRequestDto MapToDto(this NoteRequest request)
    {
        var response = new NoteRequestDto
        (
            TagsCheckedRequest: request.TagsCheckedRequest,
            TitleRequest: request.TitleRequest,
            TextRequest: request.TextRequest,
            NoteIdExchange: request.NoteIdExchange
        );

        return response;
    }

    /// <summary>
    /// Маппинг из result dto в response model
    /// </summary>
    public static NoteResponse MapFromDto(this NoteResultDto requestDto)
    {
        var response = new NoteResponse
        {
            TagsCheckedUncheckedResponse = requestDto.TagsCheckedUncheckedResponse,
            TitleResponse = requestDto.TitleResponse,
            TextResponse = requestDto.TextResponse,
            StructuredTagsListResponse = requestDto.StructuredTagsListResponse,
            NoteIdExchange = requestDto.NoteIdExchange,
            CommonErrorMessageResponse = requestDto.CommonErrorMessageResponse,
        };

        return response;
    }
}
