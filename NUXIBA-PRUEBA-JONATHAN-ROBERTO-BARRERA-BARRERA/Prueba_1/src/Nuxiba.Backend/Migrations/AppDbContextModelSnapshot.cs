using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Nuxiba.Backend.Data;
using Nuxiba.Backend.Models;

#nullable disable

namespace Nuxiba.Backend.Migrations;

[DbContext(typeof(AppDbContext))]
partial class AppDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.11");
        modelBuilder.Entity<CcArea>(b =>
        {
            b.Property<int>("IDArea").HasColumnType("int");
            b.Property<string>("AreaName").HasMaxLength(150).HasColumnType("nvarchar(150)");
            b.Property<int?>("StatusArea").HasColumnType("int");
            b.Property<DateTime?>("CreateDate").HasColumnType("datetime2");
            b.HasKey("IDArea", "AreaName");
            b.ToTable("ccRIACat_Areas");
        });
        modelBuilder.Entity<CcUser>(b =>
        {
            b.Property<int>("UserId").HasColumnName("User_id").HasColumnType("int").ValueGeneratedNever();
            b.Property<string>("Login").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<string>("Nombres").HasMaxLength(150).HasColumnType("nvarchar(150)");
            b.Property<string>("ApellidoPaterno").HasMaxLength(150).HasColumnType("nvarchar(150)");
            b.Property<string>("ApellidoMaterno").HasMaxLength(150).HasColumnType("nvarchar(150)");
            b.Property<int?>("IDArea").HasColumnType("int");
            b.HasKey("UserId");
            b.HasIndex("Login");
            b.ToTable("ccUsers");
        });
        modelBuilder.Entity<Login>(b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int").UseIdentityColumn();
            b.Property<int>("UserId").HasColumnName("User_id").HasColumnType("int");
            b.Property<int>("Extension").HasColumnType("int");
            b.Property<int>("TipoMov").HasColumnType("int");
            b.Property<DateTime>("Fecha").HasColumnName("fecha").HasColumnType("datetime2");
            b.HasKey("Id");
            b.HasIndex("UserId", "Fecha", "Id").HasDatabaseName("IX_ccloglogin_User_fecha_Id");
            b.ToTable("ccloglogin", t => t.HasCheckConstraint("CK_ccloglogin_TipoMov", "[TipoMov] IN (0,1)"));
        });
        modelBuilder.Entity<Login>().HasOne<CcUser>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Restrict).IsRequired();
    }
}
