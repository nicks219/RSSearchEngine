using System.Collections.Generic;
using System.Linq;
using SearchEngine.Data.Dto;
using SearchEngine.Data.Entities;
using SearchEngine.Service.ApiModels;

namespace SearchEngine.Service.Mapping;

/// <summary>
/// Расширения для маппинга.
/// </summary>
public static class Mapper
{
    /// <summary>
    /// Маппинг из request model в request dto.
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
    /// Маппинг из request model в request dto.
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
    /// Маппинг из result dto в response model.
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

        List<CatalogItemResponse>? MapPage(List<CatalogItemDto>? catalogItems)
        {
            return catalogItems
                ?.Select(dtoItem => new CatalogItemResponse
                (
                    Title: dtoItem.Title,
                    NoteId: dtoItem.NoteId
                )).ToList();
        }
    }

    /// <summary>
    /// Маппинг из request dto в request model.
    /// </summary>
    public static NoteRequestDto MapToDto(this NoteRequest noteRequest)
    {
        var noteRequestDto = new NoteRequestDto
        (
            CheckedTags: noteRequest.CheckedTags,
            Title: noteRequest.Title?.Trim(),
            Text: noteRequest.Text?.Trim(),
            NoteIdExchange: noteRequest.NoteIdExchange ?? 0
        );

        return noteRequestDto;
    }

    /// <summary>
    /// Маппинг из result dto в response model.
    /// </summary>
    public static NoteResponse MapFromDto(this NoteResultDto noteResultDto)
    {
        var noteResponse = new NoteResponse
        {
            CheckedUncheckedTags = noteResultDto.CheckedUncheckedTags?
                .Select(flag => flag ? "checked" : "unchecked")
                .ToList(),
            Title = noteResultDto.Title,
            Text = noteResultDto.Text,
            StructuredTags = noteResultDto.EnrichedTags,
            NoteIdExchange = noteResultDto.NoteIdExchange,
            ErrorMessage = noteResultDto.ErrorMessage,
        };

        return noteResponse;
    }

    /// <summary>
    /// Маппинг из entity в request dto.
    /// </summary>
    public static TextRequestDto MapToDto(this NoteEntity noteEntity)
    {
        var textRequestDto = new TextRequestDto
        {
            Text = noteEntity.Text,
            Title = noteEntity.Title
        };

        return textRequestDto;
    }
}
