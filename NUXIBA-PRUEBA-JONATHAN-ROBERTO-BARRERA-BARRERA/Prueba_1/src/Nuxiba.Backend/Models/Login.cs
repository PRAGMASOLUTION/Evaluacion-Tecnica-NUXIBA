using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nuxiba.Backend.Models;

[Table("ccloglogin")]
public class Login
{
    [Key]
    public int Id { get; set; }

    [Column("User_id")]
    public int UserId { get; set; }

    public int Extension { get; set; }

    public int TipoMov { get; set; }

    [Column("fecha", TypeName = "datetime2")]
    public DateTime Fecha { get; set; }
}
