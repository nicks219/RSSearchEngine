using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace SearchEngine.Data.Dto;

/// <summary>
/// Шаблон передачи данных авторизации
/// </summary>
public record LoginDto
{
    [JsonPropertyName("email"), Required] public string Email { get; set; }

    [JsonPropertyName("password"), Required] public string Password { get; set; }
}
