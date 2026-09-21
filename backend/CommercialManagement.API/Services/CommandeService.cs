using CommercialManagement.API.Data;
using CommercialManagement.API.DTOs;
using CommercialManagement.API.Enums;
using CommercialManagement.API.Helpers;
using CommercialManagement.API.Interfaces;
using CommercialManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CommercialManagement.API.Services;

public class CommandeService(ApplicationDbContext context)
    : ICommandeService
{
    public async Task<PagedResult<CommandeDto>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = BuildQuery()
            .OrderByDescending(c => c.DateCommande);

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<CommandeDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CommandeDto?> GetByIdAsync(
        int id,
        CancellationToken ct = default)
    {
        var commande = await BuildQuery()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        return commande is null
            ? null
            : ToDto(commande);
    }

    public async Task<CommandeDto> CreateAsync(
        CommandeWriteDto input,
        CancellationToken ct = default)
    {
        // Vérifier le client
        var clientExists = await context.Clients
            .AnyAsync(c => c.Id == input.ClientId, ct);

        if (!clientExists)
        {
            throw new InvalidOperationException(
                "Le client indiqué est introuvable.");
        }

        // Récupérer la TVA sélectionnée
        var tva = await context.Tvas
            .FirstOrDefaultAsync(
                t => t.Id == input.TvaId,
                ct);

        if (tva is null)
        {
            throw new InvalidOperationException(
                "La TVA sélectionnée est introuvable.");
        }

        if (!tva.Etat)
        {
            throw new InvalidOperationException(
                "La TVA sélectionnée est inactive.");
        }

        if (tva.Valeur < 0 || tva.Valeur > 100)
        {
            throw new InvalidOperationException(
                "La valeur de la TVA est invalide.");
        }

        var commande = new Commande
        {
            Numero = await GenerateNumeroAsync(ct),
            ClientId = input.ClientId,
            TvaId = input.TvaId,
            Tva = tva,
            DateCommande =
                input.DateCommande ?? DateTime.UtcNow,
            Statut = StatutCommande.Brouillon
        };

        await ApplyLinesAsync(
            commande,
            input.Lignes,
            ct);

        RecalculateTotals(commande, tva);

        context.Commandes.Add(commande);

        await context.SaveChangesAsync(ct);

        return ToDto(
            await BuildQuery()
                .FirstAsync(
                    c => c.Id == commande.Id,
                    ct));
    }

    public async Task<CommandeDto?> UpdateAsync(
        int id,
        CommandeWriteDto input,
        CancellationToken ct = default)
    {
        var commande = await BuildQuery()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (commande is null)
            return null;

        if (commande.Statut != StatutCommande.Brouillon)
        {
            throw new InvalidOperationException(
                "Seule une commande en statut Brouillon peut être modifiée.");
        }

        // Vérifier le client
        var clientExists = await context.Clients
            .AnyAsync(c => c.Id == input.ClientId, ct);

        if (!clientExists)
        {
            throw new InvalidOperationException(
                "Le client indiqué est introuvable.");
        }

        // Vérifier la TVA
        var tva = await context.Tvas
            .FirstOrDefaultAsync(
                t => t.Id == input.TvaId,
                ct);

        if (tva is null)
        {
            throw new InvalidOperationException(
                "La TVA sélectionnée est introuvable.");
        }

        if (!tva.Etat)
        {
            throw new InvalidOperationException(
                "La TVA sélectionnée est inactive.");
        }

        commande.ClientId = input.ClientId;
        commande.TvaId = input.TvaId;
        commande.Tva = tva;

        commande.DateCommande =
            input.DateCommande ?? commande.DateCommande;

        context.LignesCommande.RemoveRange(
            commande.Lignes);

        commande.Lignes.Clear();

        await ApplyLinesAsync(
            commande,
            input.Lignes,
            ct);

        RecalculateTotals(commande, tva);

        commande.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);

        return ToDto(
            await BuildQuery()
                .FirstAsync(
                    c => c.Id == id,
                    ct));
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken ct = default)
    {
        var commande = await context.Commandes
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (commande is null)
            return false;

        if (commande.Statut != StatutCommande.Brouillon)
        {
            throw new InvalidOperationException(
                "Seule une commande en statut Brouillon peut être supprimée.");
        }

        context.Commandes.Remove(commande);

        await context.SaveChangesAsync(ct);

        return true;
    }

    public async Task<CommandeDto?> ValidateAsync(
        int id,
        CancellationToken ct = default)
    {
        var commande = await BuildQuery()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (commande is null)
            return null;

        if (commande.Statut != StatutCommande.Brouillon)
        {
            throw new InvalidOperationException(
                "Cette commande est déjà validée ou annulée.");
        }

        if (commande.Lignes.Count == 0)
        {
            throw new InvalidOperationException(
                "Une commande doit contenir au moins une ligne.");
        }

        foreach (var ligne in commande.Lignes)
        {
            if (ligne.Produit.Stock < ligne.Quantite)
            {
                throw new InvalidOperationException(
                    $"Stock insuffisant pour « {ligne.Produit.Libelle} » " +
                    $"(disponible : {ligne.Produit.Stock}, " +
                    $"demandé : {ligne.Quantite}).");
            }
        }

        foreach (var ligne in commande.Lignes)
        {
            ligne.Produit.Stock -= ligne.Quantite;
        }

        commande.Statut = StatutCommande.Validee;
        commande.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);

        return ToDto(
            await BuildQuery()
                .FirstAsync(c => c.Id == id, ct));
    }

    public async Task<CommandeDto?> CancelAsync(
        int id,
        CancellationToken ct = default)
    {
        var commande = await BuildQuery()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (commande is null)
            return null;

        if (commande.Statut == StatutCommande.Annulee)
        {
            throw new InvalidOperationException(
                "Cette commande est déjà annulée.");
        }

        if (commande.Statut == StatutCommande.Livree)
        {
            throw new InvalidOperationException(
                "Une commande livrée ne peut pas être annulée.");
        }

        if (commande.Statut is not
            (StatutCommande.Brouillon or StatutCommande.Validee))
        {
            throw new InvalidOperationException(
                "Cette commande ne peut pas être annulée.");
        }

        if (commande.Remise > commande.Total)
        {
            throw new InvalidOperationException(
                "La remise ne doit pas être supérieure au total.");
        }

        if (commande.Remise < 0)
        {
            throw new InvalidOperationException(
                "La remise doit être positive.");
        }

        await using var transaction =
            await context.Database
                .BeginTransactionAsync(ct);

        if (commande.Statut == StatutCommande.Validee)
        {
            foreach (var ligne in commande.Lignes)
            {
                var quantite = ligne.Quantite;

                await context.Produits
                    .Where(p => p.Id == ligne.ProduitId)
                    .ExecuteUpdateAsync(
                        setters => setters.SetProperty(
                            p => p.Stock,
                            p => p.Stock + quantite),
                        ct);
            }
        }

        await context.Commandes
            .Where(c => c.Id == id)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        c => c.Statut,
                        StatutCommande.Annulee)
                    .SetProperty(
                        c => c.UpdatedAt,
                        DateTime.UtcNow),
                ct);

        await transaction.CommitAsync(ct);

        context.ChangeTracker.Clear();

        return ToDto(
            await BuildQuery()
                .FirstAsync(c => c.Id == id, ct));
    }

    private IQueryable<Commande> BuildQuery()
    {
        return context.Commandes
            .Include(c => c.Client)
            .Include(c => c.Tva)
            .Include(c => c.Lignes)
                .ThenInclude(l => l.Produit);
    }

    private async Task ApplyLinesAsync(
        Commande commande,
        IEnumerable<CommandeLigneWriteDto> inputs,
        CancellationToken ct)
    {
        var lines = inputs.ToList();

        if (lines.Count == 0)
        {
            throw new InvalidOperationException(
                "Une commande doit contenir au moins une ligne.");
        }

        var productIds = lines
            .Select(l => l.ProduitId)
            .Distinct()
            .ToList();

        var products = await context.Produits
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        if (products.Count != productIds.Count)
        {
            throw new InvalidOperationException(
                "Un ou plusieurs produits sont introuvables.");
        }

        foreach (var line in lines)
        {
            if (line.Quantite <= 0)
            {
                throw new InvalidOperationException(
                    $"La quantité doit être supérieure à zéro " +
                    $"(produit ID {line.ProduitId}).");
            }

            var product = products[line.ProduitId];

            if (product.Stock < line.Quantite)
            {
                throw new InvalidOperationException(
                    $"Stock insuffisant pour « {product.Libelle} » " +
                    $"(disponible : {product.Stock}, " +
                    $"demandé : {line.Quantite}).");
            }

            commande.Lignes.Add(
                new LigneCommande
                {
                    ProduitId = product.Id,
                    Produit = product,
                    Quantite = line.Quantite,
                    PrixUnitaire = product.PrixUnitaire
                });
        }
    }

    private static void RecalculateTotals(
        Commande commande,
        Tva tva)
    {
        commande.Total = commande.Lignes
            .Sum(l => l.PrixUnitaire * l.Quantite);

        var montantTva =
            Math.Round(
                commande.Total * tva.Valeur / 100m,
                2);

        commande.TotalTtc =
            Math.Round(
                commande.Total +
                montantTva -
                commande.Remise,
                2);
    }

    private async Task<string> GenerateNumeroAsync(
        CancellationToken ct)
    {
        var nextId =
            (await context.Commandes
                .MaxAsync(
                    c => (int?)c.Id,
                    ct) ?? 0) + 1;

        return $"CMD-{DateTime.UtcNow:yyyyMMdd}-{nextId:0000}";
    }

    private static CommandeDto ToDto(
        Commande commande)
    {
        var totalHt = commande.Total;

        var tvaMontant = commande.Tva is null
            ? 0
            : Math.Round(
                totalHt *
                commande.Tva.Valeur /
                100m,
                2);

        return new CommandeDto
        {
            Id = commande.Id,

            Numero = commande.Numero,

            ClientId = commande.ClientId,

            ClientNom =
                commande.Client?.Nom ?? string.Empty,

            DateCommande = commande.DateCommande,

            Statut = commande.Statut,

            TvaId = commande.TvaId,

            TvaLibelle =
                commande.Tva?.Libelle ?? string.Empty,

            Tva = tvaMontant,

            TotalHt = totalHt,

            TotalTtc = commande.TotalTtc,

            Remise = commande.Remise,

            CreatedAt = commande.CreatedAt,

            UpdatedAt = commande.UpdatedAt,

            Lignes = commande.Lignes
                .Select(l => new CommandeLigneDto
                {
                    Id = l.Id,

                    ProduitId = l.ProduitId,

                    ReferenceProduit =
                        l.Produit.Reference,

                    LibelleProduit =
                        l.Produit.Libelle,

                    Quantite = l.Quantite,

                    PrixUnitaire =
                        l.PrixUnitaire,

                    TotalLigne =
                        l.PrixUnitaire *
                        l.Quantite
                })
                .ToList()
        };
    }
}