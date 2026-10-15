using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nuxiba.Backend.Data;
using Nuxiba.Backend.DTOs;
using Nuxiba.Backend.Models;
using Nuxiba.Backend.Services;

namespace Nuxiba.Backend.Controllers;

[ApiController]
[Route("logins")]
public sealed class LoginsController(AppDbContext db, ISessionService sessions) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Login>>> GetAll(CancellationToken ct) =>
        Ok(await db.Logins.AsNoTracking().OrderBy(x => x.Fecha).ThenBy(x => x.Id).ToListAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Login>> GetById(int id, CancellationToken ct)
    {
        var item = await db.Logins.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create(LoginRequest request, CancellationToken ct)
    {
        var result = await sessions.CreateAsync(request, ct);
        if (result.Error is not null)
        {
            if (IsInputError(result.Error))
                return BadRequest(new { error = result.Error });
            return Conflict(new ProblemDetails { Title = "Secuencia de sesión inválida", Detail = result.Error, Status = 409 });
        }
        if (result.Value is null) return BadRequest(new { error = "No fue posible crear el registro." });
        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, LoginRequest request, CancellationToken ct)
    {
        var result = await sessions.UpdateAsync(id, request, ct);
        if (result.NotFound) return NotFound();
        if (result.Error is not null)
        {
            if (IsInputError(result.Error))
                return BadRequest(new { error = result.Error });
            return Conflict(new ProblemDetails { Title = "Secuencia de sesión inválida", Detail = result.Error, Status = 409 });
        }
        if (result.Value is null) return BadRequest(new { error = "La solicitud no es válida." });
        return Ok(result.Value);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (!await db.Logins.AnyAsync(x => x.Id == id, ct)) return NotFound();
        var deleted = await sessions.DeleteAsync(id, ct);
        if (!deleted) return Conflict(new { error = "La eliminación dejaría una secuencia de sesiones inválida." });
        return NoContent();
    }

    private static bool IsInputError(string error) =>
        error.Contains("no existe", StringComparison.OrdinalIgnoreCase) ||
        error.Contains("debe ser", StringComparison.OrdinalIgnoreCase) ||
        error.Contains("obligatoria", StringComparison.OrdinalIgnoreCase) ||
        error.Contains("TipoMov", StringComparison.OrdinalIgnoreCase) ||
        error.Contains("fecha", StringComparison.OrdinalIgnoreCase);
}
