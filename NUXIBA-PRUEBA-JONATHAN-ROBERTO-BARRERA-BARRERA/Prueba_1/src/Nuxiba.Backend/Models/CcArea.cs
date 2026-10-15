using System.ComponentModel.DataAnnotations.Schema;

namespace Nuxiba.Backend.Models;

// Clave compuesta para preservar el IDArea duplicado que aparece en el Excel.
[Table("ccRIACat_Areas")]
public class CcArea
{
    public int IDArea { get; set; }
    public string AreaName { get; set; } = "";
    public int? StatusArea { get; set; }
    public DateTime? CreateDate { get; set; }
}
