using System.ComponentModel.DataAnnotations;

namespace SearchEngine.Data.Dto;

/// <summary>
/// Контейнер для обновления данных авторизации
/// </summary>
public class UpdateCredosRequest
{
    [Required] public required LoginDto OldCredos {get; set;}
    [Required] public required LoginDto NewCredos {get; set;}
}
