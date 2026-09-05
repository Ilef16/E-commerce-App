using CommercialManagement.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommercialManagement.API.Data.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");

        builder.Property(c => c.Nom).HasMaxLength(120).IsRequired();
        builder.Property(c => c.Identifiant).HasMaxLength(30).IsRequired();
        builder.Property(c => c.Prenom).HasMaxLength(80);
        builder.Property(c => c.Email).HasMaxLength(180).IsRequired();
        builder.Property(c => c.Telephone).HasMaxLength(30);
        builder.Property(c => c.Adresse).HasMaxLength(250);
        builder.Property(c => c.Ville).HasMaxLength(100);
        builder.Property(c => c.CodePostal).HasMaxLength(20);

        builder.HasIndex(c => c.Email).IsUnique();
        builder.HasIndex(c => c.Identifiant).IsUnique();
    }
}
