using CommercialManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CommercialManagement.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Client> Clients { get; set; } = null!;
    public DbSet<Produit> Produits { get; set; } = null!;
    public DbSet<Commande> Commandes { get; set; } = null!;
    public DbSet<LigneCommande> LignesCommande { get; set; } = null!;
    public DbSet<Tva> Tvas { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

      modelBuilder.Entity<Commande>(entity =>
{
    entity.Property(c => c.Remise)
        .HasPrecision(18, 2);

    entity.Property(c => c.Total)
        .HasPrecision(18, 2);

    entity.Property(c => c.TotalTtc)
        .HasPrecision(18, 2);
});

modelBuilder.Entity<LigneCommande>(entity =>
{
    entity.Property(l => l.PrixUnitaire)
        .HasPrecision(18, 2);

    entity.Property(l => l.TotalLigne)
        .HasPrecision(18, 2)
        .HasComputedColumnSql("[Quantite] * [PrixUnitaire]");
});

modelBuilder.Entity<Produit>(entity =>
{
    entity.Property(p => p.PrixUnitaire)
        .HasPrecision(18, 2);
});

modelBuilder.Entity<Tva>(entity =>
{
    entity.Property(t => t.Valeur)
        .HasPrecision(5, 2);
});
    }
}