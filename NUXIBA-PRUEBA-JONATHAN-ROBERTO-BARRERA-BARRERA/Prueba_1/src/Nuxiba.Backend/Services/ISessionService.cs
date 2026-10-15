using Nuxiba.Backend.DTOs;
using Nuxiba.Backend.Models;

namespace Nuxiba.Backend.Services;

public interface ISessionService
{
    Task<(Login? Value, string? Error, bool NotFound)> CreateAsync(LoginRequest request, CancellationToken ct);
    Task<(Login? Value, string? Error, bool NotFound)> UpdateAsync(int id, LoginRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
