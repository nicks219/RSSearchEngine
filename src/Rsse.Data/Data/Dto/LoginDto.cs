using System.ComponentModel.DataAnnotations;

namespace RandomSongSearchEngine.Data.DTO;

public record LoginDto
{
    [Required(ErrorMessage = "[Empty email]")]
    public string? Email { get; }

    [Required(ErrorMessage = "[Empty password]")]
    [DataType(DataType.Password)]
    public string? Password { get; }

    public LoginDto(string email, string password)
    {
        Email = email;
        
        Password = password;
    }
}
