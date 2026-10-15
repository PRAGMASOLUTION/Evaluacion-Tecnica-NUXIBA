using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nuxiba.Backend.Data;

#nullable disable

namespace Nuxiba.Backend.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("202610150001_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ccRIACat_Areas",
            columns: table => new
            {
                IDArea = table.Column<int>(type: "int", nullable: false),
                AreaName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                StatusArea = table.Column<int>(type: "int", nullable: true),
                CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_ccRIACat_Areas", x => new { x.IDArea, x.AreaName }));

        migrationBuilder.CreateTable(
            name: "ccUsers",
            columns: table => new
            {
                User_id = table.Column<int>(type: "int", nullable: false),
                Login = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Nombres = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                ApellidoPaterno = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                ApellidoMaterno = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                IDArea = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_ccUsers", x => x.User_id));

        migrationBuilder.CreateTable(
            name: "ccloglogin",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                User_id = table.Column<int>(type: "int", nullable: false),
                Extension = table.Column<int>(type: "int", nullable: false),
                TipoMov = table.Column<int>(type: "int", nullable: false),
                fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ccloglogin", x => x.Id);
                table.CheckConstraint("CK_ccloglogin_TipoMov", "[TipoMov] IN (0,1)");
                table.ForeignKey("FK_ccloglogin_ccUsers_User_id", x => x.User_id, "ccUsers", "User_id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(name: "IX_ccloglogin_User_fecha_Id", table: "ccloglogin", columns: new[] { "User_id", "fecha", "Id" });
        migrationBuilder.CreateIndex(name: "IX_ccUsers_Login", table: "ccUsers", column: "Login");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("ccloglogin");
        migrationBuilder.DropTable("ccUsers");
        migrationBuilder.DropTable("ccRIACat_Areas");
    }
}
