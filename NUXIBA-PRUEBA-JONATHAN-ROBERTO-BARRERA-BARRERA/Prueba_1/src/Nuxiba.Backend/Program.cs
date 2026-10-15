using Microsoft.EntityFrameworkCore;
using Nuxiba.Backend.Data;
using Nuxiba.Backend.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<CsvReportService>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.MapControllers();
app.MapGet("/", () => Results.Ok(new { application = "NUXIBA Backend API", docs = "/swagger" }));
app.Run();

public partial class Program { }
