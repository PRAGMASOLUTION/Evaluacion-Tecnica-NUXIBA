using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nuxiba.Backend.Models;

[Table("ccUsers")]
public class CcUser
{
    [Key]
    [Column("User_id")]
    public int UserId { get; set; }

    [Required, MaxLength(100)]
    public string Login { get; set; } = "";

    [MaxLength(150)]
    public string? Nombres { get; set; }

    [MaxLength(150)]
    public string? ApellidoPaterno { get; set; }

    [MaxLength(150)]
    public string? ApellidoMaterno { get; set; }

    public int? IDArea { get; set; }
}
