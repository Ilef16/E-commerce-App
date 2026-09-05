using CommercialManagement.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialManagement.API.Data.Configurations;

public class CommandeConfiguration : IEntityTypeConfiguration<Commande>
{
    public void Configure(EntityTypeBuilder<Commande> builder)
    {
        builder.ToTable("Commandes");

        builder.Property(c => c.Numero).HasMaxLength(30).IsRequired();
        builder.Property(c => c.Total).HasColumnType("decimal(18,2)");

        builder.HasIndex(c => c.Numero).IsUnique();

        builder.HasOne(c => c.Client)
            .WithMany(client => client.Commandes)
            .HasForeignKey(c => c.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Lignes)
            .WithOne(l => l.Commande)
            .HasForeignKey(l => l.CommandeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
