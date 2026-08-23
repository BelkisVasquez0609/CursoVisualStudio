using Beneficiarios360.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beneficiarios360.Api.Data
{
    public sealed class AppDbContext(
     DbContextOptions<AppDbContext> options)
     : DbContext(options)
    {
        public DbSet<Beneficiario> Beneficiarios =>
            Set<Beneficiario>();

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            var entity =
                modelBuilder.Entity<Beneficiario>();

            entity.ToTable("Beneficiarios");
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.Documento).IsUnique();

            entity.Property(x => x.Documento).HasMaxLength(20).IsRequired();

            entity.Property(x => x.Nombres).HasMaxLength(100) .IsRequired();

            entity.Property(x => x.Apellidos).HasMaxLength(100) .IsRequired();

            entity.Property(x => x.Activo).HasDefaultValue(true);

            entity.Property(x => x.CreadoUtc).HasDefaultValueSql("SYSUTCDATETIME()");
        }
    }
}
