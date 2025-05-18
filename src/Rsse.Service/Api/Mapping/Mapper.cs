using System.Collections.Generic;
using System.Linq;
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
    public static UpdateCredosRequestDto MapToDto(this UpdateCredentialsRequest updateCredosRequest)
    {
        var updateCredosRequestDto = new UpdateCredosRequestDto
        {
            NewCredos = new CredentialsRequestDto
            {
                Email = updateCredosRequest.NewCredos.Email,
                Password = updateCredosRequest.NewCredos.Password
            },
            OldCredos = new CredentialsRequestDto
            {
                Email = updateCredosRequest.OldCredos.Email,
                Password = updateCredosRequest.OldCredos.Password
            }
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
        var catalogResponse = new CatalogResponse
        {
            CatalogPage = MapPage(catalogResultDto.CatalogPage),
            NotesCount = catalogResultDto.NotesCount,
            PageNumber = catalogResultDto.PageNumber,
            ErrorMessage = catalogResultDto.ErrorMessage
        };

        return catalogResponse;

        List<CatalogItemResponse>? MapPage(List<CatalogItemDto>? dtoItems)
        {
            return dtoItems
                ?.Select(dtoItem => new CatalogItemResponse
                {
                    Title = dtoItem.Title,
                    NoteId = dtoItem.NoteId
                }).ToList();
        }
    }

    /// <summary>
    /// Маппинг из request dto в request model
    /// </summary>
    public static NoteRequestDto MapToDto(this NoteRequest noteRequest)
    {
        var noteRequestDto = new NoteRequestDto
        (
            CheckedTags: noteRequest.CheckedTags,
            Title: noteRequest.Title,
            Text: noteRequest.Text,
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
            CheckedUncheckedTags = noteResultDto.CheckedUncheckedTags,
            Title = noteResultDto.Title,
            Text = noteResultDto.Text,
            StructuredTags = noteResultDto.StructuredTags,
            NoteIdExchange = noteResultDto.NoteIdExchange,
            ErrorMessage = noteResultDto.ErrorMessage,
        };

        return noteResponse;
    }
}
