using System.ComponentModel.DataAnnotations;

namespace Nuxiba.Backend.DTOs;

public sealed class LoginRequest
{
    [Range(1, int.MaxValue)]
    public int User_id { get; init; }

    public int Extension { get; init; }

    [Range(0, 1, ErrorMessage = "TipoMov solo puede ser 0 (logout) o 1 (login).")]
    public int TipoMov { get; init; }

    public DateTime fecha { get; init; }
}
