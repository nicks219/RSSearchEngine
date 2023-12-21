using System.ComponentModel.DataAnnotations;

namespace SearchEngine.Data;

public class UserEntity
{
    public int Id { get; set; }

    [MaxLength(30)]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Password { get; set; }
}
