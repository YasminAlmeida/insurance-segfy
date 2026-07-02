using Microsoft.EntityFrameworkCore;
using SegfyInsurance.Domain.Entities;

namespace SegfyInsurance.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<InsurancePolicy> Policies => Set<InsurancePolicy>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<InsurancePolicy>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasIndex(e => e.NumeroApolice).IsUnique();
                entity.HasIndex(e => e.CpfCnpjSegurado);
                entity.HasIndex(e => e.DataFimVigencia);
                entity.Property(e => e.NumeroApolice).IsRequired().HasMaxLength(30);

                entity.Property(e => e.CpfCnpjSegurado).IsRequired().HasMaxLength(14);
                entity.Property(e => e.PlacaVeiculo).IsRequired().HasMaxLength(8);
                entity.Property(e => e.ValorPremio).IsRequired();
            });
        }
    }
}
