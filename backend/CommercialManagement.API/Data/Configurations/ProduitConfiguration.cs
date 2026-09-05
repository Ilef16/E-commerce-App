using CommercialManagement.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialManagement.API.Data.Configurations;

public class ProduitConfiguration : IEntityTypeConfiguration<Produit>
{
    public void Configure(EntityTypeBuilder<Produit> builder)
    {
        builder.ToTable("Produits");

        builder.Property(p => p.Reference).HasMaxLength(40).IsRequired();
        builder.Property(p => p.Libelle).HasMaxLength(160).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.PhotoUrl).HasMaxLength(500);
        builder.Property(p => p.PrixUnitaire).HasColumnType("decimal(18,2)");

        builder.HasIndex(p => p.Reference).IsUnique();
    }
}
