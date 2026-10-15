using System.Data;
using Microsoft.EntityFrameworkCore;
using Nuxiba.Backend.Data;
using Nuxiba.Backend.DTOs;
using Nuxiba.Backend.Models;

namespace Nuxiba.Backend.Services;

public sealed class SessionService(AppDbContext db) : ISessionService
{
    public async Task<(Login? Value, string? Error, bool NotFound)> CreateAsync(LoginRequest request, CancellationToken ct)
    {
        if (!IsValidRequest(request, out var error)) return (null, error, false);
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        if (!await db.Users.AnyAsync(x => x.UserId == request.User_id, ct))
            return (null, "El User_id no existe en ccUsers.", false);

        var current = await db.Logins.Where(x => x.UserId == request.User_id).OrderBy(x => x.Fecha).ThenBy(x => x.Id).ToListAsync(ct);
        var entity = ToEntity(request);
        var proposed = current.Append(entity).ToList();
        error = SessionPairing.ValidateSequence(proposed, strictEventId: 0);
        if (error is not null) return (null, error, false);

        db.Logins.Add(entity);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return (entity, null, false);
    }

    public async Task<(Login? Value, string? Error, bool NotFound)> UpdateAsync(int id, LoginRequest request, CancellationToken ct)
    {
        if (!IsValidRequest(request, out var error)) return (null, error, false);
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var entity = await db.Logins.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return (null, null, true);
        if (!await db.Users.AnyAsync(x => x.UserId == request.User_id, ct))
            return (null, "El User_id no existe en ccUsers.", false);

        var oldUserId = entity.UserId;
        var oldState = new Login { Id = entity.Id, UserId = entity.UserId, Extension = entity.Extension, TipoMov = entity.TipoMov, Fecha = entity.Fecha };
        entity.UserId = request.User_id;
        entity.Extension = request.Extension;
        entity.TipoMov = request.TipoMov;
        entity.Fecha = request.fecha;

        var affectedUsers = new HashSet<int> { oldUserId, entity.UserId };
        // Cargar ambas secuencias antes de validarlas; luego filtrar en memoria para incluir
        // correctamente la entidad modificada aunque haya cambiado de UserId.
        var affectedRecords = await db.Logins
            .Where(x => x.UserId == oldUserId || x.UserId == entity.UserId)
            .ToListAsync(ct);
        foreach (var userId in affectedUsers)
        {
            var records = affectedRecords.Where(x => x.UserId == userId)
                .OrderBy(x => x.Fecha).ThenBy(x => x.Id).ToList();
            error = SessionPairing.ValidateSequence(records, strictEventId: id);
            if (error is not null)
            {
                entity.UserId = oldState.UserId; entity.Extension = oldState.Extension;
                entity.TipoMov = oldState.TipoMov; entity.Fecha = oldState.Fecha;
                return (null, $"La actualización invalida la secuencia del usuario {userId}: {error}", false);
            }
        }
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return (entity, null, false);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var entity = await db.Logins.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return false;
        var userId = entity.UserId;
        db.Logins.Remove(entity);
        await db.SaveChangesAsync(ct);
        var records = await db.Logins.Where(x => x.UserId == userId).OrderBy(x => x.Fecha).ThenBy(x => x.Id).ToListAsync(ct);
        if (SessionPairing.ValidateSequence(records) is not null)
        {
            await tx.RollbackAsync(ct);
            return false;
        }
        await tx.CommitAsync(ct);
        return true;
    }

    private static Login ToEntity(LoginRequest r) => new()
    {
        UserId = r.User_id, Extension = r.Extension, TipoMov = r.TipoMov, Fecha = r.fecha
    };

    private static bool IsValidRequest(LoginRequest r, out string? error)
    {
        if (r.User_id <= 0) { error = "User_id debe ser mayor que cero."; return false; }
        if (r.TipoMov is not (0 or 1)) { error = "TipoMov solo puede ser 0 o 1."; return false; }
        if (r.fecha == default || r.fecha < new DateTime(1753, 1, 1) || r.fecha > new DateTime(9999, 12, 31))
        { error = "La fecha es obligatoria y debe estar dentro del rango admitido por SQL Server."; return false; }
        error = null; return true;
    }
}
