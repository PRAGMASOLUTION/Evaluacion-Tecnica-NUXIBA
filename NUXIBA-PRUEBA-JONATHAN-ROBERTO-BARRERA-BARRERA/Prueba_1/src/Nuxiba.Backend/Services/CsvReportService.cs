using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Nuxiba.Backend.Data;
using Nuxiba.Backend.Models;

namespace Nuxiba.Backend.Services;

public sealed class CsvReportService(AppDbContext db)
{
    public async Task<byte[]> GenerateAsync(CancellationToken ct)
    {
        var users = await db.Users.AsNoTracking().OrderBy(x => x.UserId).ToListAsync(ct);
        var events = await db.Logins.AsNoTracking().OrderBy(x => x.UserId).ThenBy(x => x.Fecha).ThenBy(x => x.Id).ToListAsync(ct);
        var areas = await db.Areas.AsNoTracking().OrderBy(x => x.IDArea).ThenBy(x => x.AreaName).ToListAsync(ct);
        var areaById = areas.GroupBy(x => x.IDArea).ToDictionary(g => g.Key, g => g.First().AreaName);
        var durationByUser = events.GroupBy(x => x.UserId).ToDictionary(
            g => g.Key,
            g => SessionPairing.CompleteSessions(g).Sum(pair => Math.Max(0L, (long)(pair.End.Fecha - pair.Start.Fecha).TotalSeconds)));

        var sb = new StringBuilder();
        sb.AppendLine("Usuario,NombreCompleto,Area,TotalHoras");
        foreach (var user in users)
        {
            var fullName = string.Join(" ", new[] { user.Nombres, user.ApellidoPaterno, user.ApellidoMaterno }
                .Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s!.Trim()));
            var area = user.IDArea is int id && areaById.TryGetValue(id, out var name) ? name : "";
            var seconds = durationByUser.GetValueOrDefault(user.UserId, 0L);
            var hours = (seconds / 3600d).ToString("0.####", CultureInfo.InvariantCulture);
            sb.Append(Escape(user.Login)).Append(',')
              .Append(Escape(fullName)).Append(',')
              .Append(Escape(area)).Append(',')
              .Append(hours).AppendLine();
        }

        // BOM UTF-8 mejora la apertura del archivo en Excel.
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private static string Escape(string? value)
    {
        value ??= "";
        if (value.Contains('"') || value.Contains(',') || value.Contains('\r') || value.Contains('\n'))
            return """ + value.Replace(""", """") + """;
        return value;
    }
}
