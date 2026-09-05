using CommercialManagement.API.Data;
using CommercialManagement.API.DTOs;
using CommercialManagement.API.Enums;
using CommercialManagement.API.Interfaces;
using CommercialManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CommercialManagement.API.Services;

public class CommandeService(ApplicationDbContext context) : ICommandeService
{
    public async Task<IReadOnlyList<CommandeDto>> GetAllAsync(CancellationToken ct = default)
    {
        var commandes = await context.Commandes.AsNoTracking()
            .Include(c => c.Client)
            .Include(c => c.Lignes).ThenInclude(l => l.Produit)
            .OrderByDescending(c => c.DateCommande)
            .ToListAsync(ct);
        return commandes.Select(ToDto).ToList();
    }

    public async Task<CommandeDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var commande = await Query().FirstOrDefaultAsync(c => c.Id == id, ct);
        return commande is null ? null : ToDto(commande);
    }

    public async Task<CommandeDto> CreateAsync(CommandeWriteDto input, CancellationToken ct = default)
    {
        var commande = new Commande
        {
            Numero = await GenerateNumeroAsync(ct),
            ClientId = input.ClientId,
            DateCommande = input.DateCommande ?? DateTime.UtcNow
        };

        await ApplyLinesAsync(commande, input.Lignes, ct);
        context.Commandes.Add(commande);
        await context.SaveChangesAsync(ct);
        return ToDto(await Query().FirstAsync(c => c.Id == commande.Id, ct));
    }

    public async Task<CommandeDto?> UpdateAsync(int id, CommandeWriteDto input, CancellationToken ct = default)
    {
        var commande = await Query().FirstOrDefaultAsync(c => c.Id == id, ct);
        if (commande is null) return null;
        if (commande.Statut != StatutCommande.Brouillon)
            throw new InvalidOperationException("Seule une commande brouillon peut être modifiée.");

        commande.ClientId = input.ClientId;
        commande.DateCommande = input.DateCommande ?? commande.DateCommande;
        context.LignesCommande.RemoveRange(commande.Lignes);
        commande.Lignes.Clear();
        await ApplyLinesAsync(commande, input.Lignes, ct);
        commande.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return ToDto(await Query().FirstAsync(c => c.Id == id, ct));
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var commande = await context.Commandes.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (commande is null) return false;
        if (commande.Statut != StatutCommande.Brouillon)
            throw new InvalidOperationException("Seule une commande brouillon peut être supprimée.");

        context.Commandes.Remove(commande);
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<CommandeDto?> ValidateAsync(int id, CancellationToken ct = default)
    {
        var commande = await Query().FirstOrDefaultAsync(c => c.Id == id, ct);
        if (commande is null) return null;
        if (commande.Statut != StatutCommande.Brouillon)
            throw new InvalidOperationException("Cette commande est déjà validée.");
        if (commande.Lignes.Count == 0)
            throw new InvalidOperationException("Une commande doit contenir au moins une ligne.");

        foreach (var ligne in commande.Lignes)
        {
            if (ligne.Produit.Stock < ligne.Quantite)
                throw new InvalidOperationException($"Stock insuffisant pour le produit {ligne.Produit.Reference}.");
        }

        foreach (var ligne in commande.Lignes)
            ligne.Produit.Stock -= ligne.Quantite;

        commande.Statut = StatutCommande.Validee;
        commande.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return ToDto(await Query().FirstAsync(c => c.Id == id, ct));
    }

    private IQueryable<Commande> Query() => context.Commandes
        .Include(c => c.Client)
        .Include(c => c.Lignes).ThenInclude(l => l.Produit);

    private async Task ApplyLinesAsync(Commande commande, IEnumerable<CommandeLigneWriteDto> inputs, CancellationToken ct)
    {
        var lines = inputs.ToList();
        if (lines.Count == 0) throw new InvalidOperationException("Une commande doit contenir au moins une ligne.");
        if (await context.Clients.AllAsync(c => c.Id != commande.ClientId, ct))
            throw new InvalidOperationException("Le client indiqué est introuvable.");

        var productIds = lines.Select(l => l.ProduitId).Distinct().ToList();
        var products = await context.Produits.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, ct);
        if (products.Count != productIds.Count)
            throw new InvalidOperationException("Un ou plusieurs produits sont introuvables.");

        foreach (var input in lines)
        {
            var product = products[input.ProduitId];
            commande.Lignes.Add(new LigneCommande
            {
                ProduitId = product.Id,
                Produit = product,
                Quantite = input.Quantite,
                PrixUnitaire = product.PrixUnitaire
            });
        }

        commande.Total = commande.Lignes.Sum(l => l.PrixUnitaire * l.Quantite);
    }

    private async Task<string> GenerateNumeroAsync(CancellationToken ct)
    {
        var next = (await context.Commandes.MaxAsync(c => (int?)c.Id, ct) ?? 0) + 1;
        return $"CMD-{DateTime.UtcNow:yyyyMMdd}-{next:0000}";
    }

    private static CommandeDto ToDto(Commande commande) => new()
    {
        Id = commande.Id,
        Numero = commande.Numero,
        ClientId = commande.ClientId,
        ClientNom = commande.Client.Nom,
        DateCommande = commande.DateCommande,
        Statut = commande.Statut,
        Total = commande.Total,
        CreatedAt = commande.CreatedAt,
        UpdatedAt = commande.UpdatedAt,
        Lignes = commande.Lignes.Select(l => new CommandeLigneDto
        {
            Id = l.Id,
            ProduitId = l.ProduitId,
            ReferenceProduit = l.Produit.Reference,
            LibelleProduit = l.Produit.Libelle,
            Quantite = l.Quantite,
            PrixUnitaire = l.PrixUnitaire,
            TotalLigne = l.PrixUnitaire * l.Quantite
        }).ToList()
    };
}
