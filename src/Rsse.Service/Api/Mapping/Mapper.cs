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
    public static UpdateCredosRequestDto MapToDto(this UpdateCredentialsRequest updateRequest)
    {
        var updateCredosRequestDto = new UpdateCredosRequestDto
        {
            NewCredos = new CredentialsRequestDto { Email = updateRequest.NewCredos.Email, Password = updateRequest.NewCredos.Password },
            OldCredos = new CredentialsRequestDto { Email = updateRequest.OldCredos.Email, Password = updateRequest.OldCredos.Password }
        };

        return updateCredosRequestDto;
    }

    /// <summary>
    /// Маппинг из request model в request dto
    /// </summary>
    public static CatalogRequestDto MapToDto(this CatalogRequest catalogRequest)
    {
        var catalogRequestDto = new CatalogRequestDto
        {
            PageNumber = catalogRequest.PageNumber,
            Direction = catalogRequest.Direction
        };

        return catalogRequestDto;
    }

    /// <summary>
    /// Маппинг из result dto в response model
    /// </summary>
    public static CatalogResponse MapFromDto(this CatalogResultDto catalogResultDto)
    {
        var catalogResponseDto = new CatalogResponse
        (
            CatalogPage: catalogResultDto.CatalogPage,
            NotesCount: catalogResultDto.NotesCount,
            PageNumber: catalogResultDto.PageNumber
        );

        return catalogResponseDto;
    }

    /// <summary>
    /// Маппинг из request dto в request model
    /// </summary>
    public static NoteRequestDto MapToDto(this NoteRequest noteRequest)
    {
        var noteRequestDto = new NoteRequestDto
        (
            TagsCheckedRequest: noteRequest.TagsCheckedRequest,
            TitleRequest: noteRequest.TitleRequest,
            TextRequest: noteRequest.TextRequest,
            NoteIdExchange: noteRequest.NoteIdExchange
        );

        return noteRequestDto;
    }

    /// <summary>
    /// Маппинг из result dto в response model
    /// </summary>
    public static NoteResponse MapFromDto(this NoteResultDto noteResultDto)
    {
        var noteResponse = new NoteResponse
        {
            TagsCheckedUncheckedResponse = noteResultDto.TagsCheckedUncheckedResponse,
            TitleResponse = noteResultDto.TitleResponse,
            TextResponse = noteResultDto.TextResponse,
            StructuredTagsListResponse = noteResultDto.StructuredTagsListResponse,
            NoteIdExchange = noteResultDto.NoteIdExchange,
            CommonErrorMessageResponse = noteResultDto.CommonErrorMessageResponse,
        };

        return noteResponse;
    }
}
