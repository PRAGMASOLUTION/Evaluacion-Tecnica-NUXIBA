using Microsoft.EntityFrameworkCore;
using Nuxiba.Backend.Models;

namespace Nuxiba.Backend.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Login> Logins => Set<Login>();
    public DbSet<CcUser> Users => Set<CcUser>();
    public DbSet<CcArea> Areas => Set<CcArea>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Login>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).UseIdentityColumn();
            entity.Property(x => x.Fecha).HasColumnType("datetime2");
            entity.HasIndex(x => new { x.UserId, x.Fecha, x.Id })
                .HasDatabaseName("IX_ccloglogin_User_fecha_Id");
            entity.HasCheckConstraint("CK_ccloglogin_TipoMov", "[TipoMov] IN (0,1)");
            entity.HasOne<CcUser>().WithMany().HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CcUser>(entity =>
        {
            entity.HasKey(x => x.UserId);
            entity.Property(x => x.UserId).ValueGeneratedNever();
            entity.Property(x => x.Login).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.Login);
        });

        modelBuilder.Entity<CcArea>(entity =>
        {
            entity.HasKey(x => new { x.IDArea, x.AreaName });
            entity.Property(x => x.AreaName).HasMaxLength(150);
        });
    }
}
