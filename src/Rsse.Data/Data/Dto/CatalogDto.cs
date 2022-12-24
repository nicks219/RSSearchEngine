using System.Text.Json.Serialization;

namespace RandomSongSearchEngine.Data.DTO;

public record CatalogDto
{
    // название и соответствующее ему Id из бд
    [JsonPropertyName("titlesAndIds")] 
    public List<Tuple<string, int>>? CatalogPage { get; init; }

    [JsonPropertyName("navigationButtons")]
    public List<int>? NavigationButtons { get; init; }

    [JsonPropertyName("songsCount")] 
    public int SongsCount { get; init; }

    [JsonPropertyName("pageNumber")] 
    public int PageNumber { get; init; }

    [JsonPropertyName("errorMessage")] 
    public string? ErrorMessage { get; init; }

    public int Direction()
    {
        if (NavigationButtons is null)
        {
            return 0;
        }
        
        return NavigationButtons[0] switch
        {
            1 => 1,
            2 => 2,
            _ => throw new NotImplementedException("[Wrong Navigate]")
        };
    }
}