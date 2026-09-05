using CommercialManagement.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialManagement.API.Data.Configurations;

public class LigneCommandeConfiguration : IEntityTypeConfiguration<LigneCommande>
{
    public void Configure(EntityTypeBuilder<LigneCommande> builder)
    {
        builder.ToTable("LignesCommande");

        builder.Property(l => l.PrixUnitaire).HasColumnType("decimal(18,2)");
        builder.Property(l => l.TotalLigne)
            .HasColumnType("decimal(18,2)")
            .HasComputedColumnSql("[Quantite] * [PrixUnitaire]", stored: true);

        builder.HasOne(l => l.Produit)
            .WithMany(p => p.LignesCommande)
            .HasForeignKey(l => l.ProduitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
