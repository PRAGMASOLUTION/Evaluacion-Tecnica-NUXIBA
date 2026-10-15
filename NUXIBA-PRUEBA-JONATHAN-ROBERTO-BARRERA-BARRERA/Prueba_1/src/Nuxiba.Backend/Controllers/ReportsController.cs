using Microsoft.AspNetCore.Mvc;
using Nuxiba.Backend.Services;

namespace Nuxiba.Backend.Controllers;

[ApiController]
[Route("reports")]
public sealed class ReportsController(CsvReportService reports) : ControllerBase
{
    [HttpGet("hours.csv")]
    [Produces("text/csv")]
    public async Task<IActionResult> HoursCsv(CancellationToken ct)
    {
        var bytes = await reports.GenerateAsync(ct);
        return File(bytes, "text/csv; charset=utf-8", $"nuxiba-horas-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv");
    }
}
