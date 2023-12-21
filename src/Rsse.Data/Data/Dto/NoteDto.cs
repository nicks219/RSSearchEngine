using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SearchEngine.Data.Dto;

public record NoteDto
{
    // request:
    [JsonPropertyName("checkedCheckboxesJs")]
    public List<int>? TagsCheckedRequest { get; set; }

    [JsonPropertyName("titleJs")]
    public string? TitleRequest { get; set; }

    [JsonPropertyName("textJs")]
    public string? TextRequest { get; set; }

    // response:
    [JsonPropertyName("isGenreCheckedCS")]
    public List<string>? TagsCheckedUncheckedResponse { get; init; }

    [JsonPropertyName("titleCS")]
    public string? TitleResponse { get; set; }

    [JsonPropertyName("textCS")]
    public string? TextResponse { get; set; }

    // common:
    [JsonPropertyName("genresNamesCS")]
    public List<string>? CommonTagsListResponse { get; init; }

    [JsonPropertyName("savedTextId")]
    public int NoteId { get; set; }

    public string? ErrorMessageResponse { get; set; }

    public NoteDto()
    {
    }

    public NoteDto(
        List<string> commonTagsListResponse,
        int noteId = 0,
        string textResponse = "",
        string titleResponse = "",
        List<string>? tagsCheckedUncheckedResponse = null)
    {
        TextResponse = textResponse;
        TitleResponse = titleResponse;
        TagsCheckedUncheckedResponse = tagsCheckedUncheckedResponse ?? new List<string>();
        CommonTagsListResponse = commonTagsListResponse;
        NoteId = noteId;
    }
}
