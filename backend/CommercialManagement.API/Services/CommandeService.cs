using CommercialManagement.API.Constants;
using CommercialManagement.API.Data;
using CommercialManagement.API.DTOs;
using CommercialManagement.API.Enums;
using CommercialManagement.API.Interfaces;
using CommercialManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CommercialManagement.API.Services;

public class CommandeService(ApplicationDbContext context) : ICommandeService
{

    // ── Queries ────────────────────────────────────────────────────────────

    /// <summary>Returns a paginated page of orders, most recent first.</summary>
    public async Task<PagedResult<CommandeDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = context.Commandes.AsNoTracking()
            .Include(c => c.Client)
            .Include(c => c.Lignes).ThenInclude(l => l.Produit)
            .OrderByDescending(c => c.DateCommande);

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<CommandeDto>
        {
            Items    = items.Select(ToDto).ToList(),
            TotalCount = total,
            Page     = page,
            PageSize = pageSize,
        };
    }

    public async Task<CommandeDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var commande = await BuildQuery().FirstOrDefaultAsync(c => c.Id == id, ct);
        return commande is null ? null : ToDto(commande);
    }

    // ── Create ─────────────────────────────────────────────────────────────

    public async Task<CommandeDto> CreateAsync(CommandeWriteDto input, CancellationToken ct = default)
    {
        var commande = new Commande
        {
            Numero = await GenerateNumeroAsync(ct),
            ClientId = input.ClientId,
            DateCommande = input.DateCommande ?? DateTime.UtcNow,
        };

        await ApplyLinesAsync(commande, input.Lignes, ct);

        context.Commandes.Add(commande);
        await context.SaveChangesAsync(ct);

        return ToDto(await BuildQuery().FirstAsync(c => c.Id == commande.Id, ct));
    }

    // ── Update ─────────────────────────────────────────────────────────────

    public async Task<CommandeDto?> UpdateAsync(int id, CommandeWriteDto input, CancellationToken ct = default)
    {
        var commande = await BuildQuery().FirstOrDefaultAsync(c => c.Id == id, ct);
        if (commande is null) return null;
        if (commande.Statut != StatutCommande.Brouillon)
            throw new InvalidOperationException("Seule une commande en statut Brouillon peut être modifiée.");

        commande.ClientId = input.ClientId;
        commande.DateCommande = input.DateCommande ?? commande.DateCommande;

        context.LignesCommande.RemoveRange(commande.Lignes);
        commande.Lignes.Clear();

        await ApplyLinesAsync(commande, input.Lignes, ct);

        commande.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);

        return ToDto(await BuildQuery().FirstAsync(c => c.Id == id, ct));
    }

    // ── Delete ─────────────────────────────────────────────────────────────

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var commande = await context.Commandes.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (commande is null) return false;
        if (commande.Statut != StatutCommande.Brouillon)
            throw new InvalidOperationException("Seule une commande en statut Brouillon peut être supprimée.");

        context.Commandes.Remove(commande);
        await context.SaveChangesAsync(ct);
        return true;
    }

    // ── Validate ───────────────────────────────────────────────────────────

    public async Task<CommandeDto?> ValidateAsync(int id, CancellationToken ct = default)
    {
        var commande = await BuildQuery().FirstOrDefaultAsync(c => c.Id == id, ct);
        if (commande is null) return null;
        if (commande.Statut != StatutCommande.Brouillon)
            throw new InvalidOperationException("Cette commande est déjà validée ou annulée.");
        if (commande.Lignes.Count == 0)
            throw new InvalidOperationException("Une commande doit contenir au moins une ligne.");

        // Verify stock once more at validation time (stock may have changed since creation)
        foreach (var ligne in commande.Lignes)
        {
            if (ligne.Quantite <= 0)
                throw new InvalidOperationException(
                    $"La quantité de la ligne {ligne.Produit.Reference} doit être supérieure à zéro.");

            if (ligne.Produit.Stock < ligne.Quantite)
                throw new InvalidOperationException(
                    $"Stock insuffisant pour « {ligne.Produit.Libelle} » " +
                    $"(disponible : {ligne.Produit.Stock}, demandé : {ligne.Quantite}).");
        }

        // Decrement stock atomically
        foreach (var ligne in commande.Lignes)
            ligne.Produit.Stock -= ligne.Quantite;

        commande.Statut = StatutCommande.Validee;
        commande.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);

        return ToDto(await BuildQuery().FirstAsync(c => c.Id == id, ct));
    }

    // ── Private helpers ────────────────────────────────────────────────────

    private IQueryable<Commande> BuildQuery() =>
        context.Commandes
            .Include(c => c.Client)
            .Include(c => c.Lignes).ThenInclude(l => l.Produit);

    private async Task ApplyLinesAsync(
        Commande commande,
        IEnumerable<CommandeLigneWriteDto> inputs,
        CancellationToken ct)
    {
        var lines = inputs.ToList();

        if (lines.Count == 0)
            throw new InvalidOperationException("Une commande doit contenir au moins une ligne.");

        // Validate client exists
        var clientExists = await context.Clients.AnyAsync(c => c.Id == commande.ClientId, ct);
        if (!clientExists)
            throw new InvalidOperationException("Le client indiqué est introuvable.");

        // Validate each line quantity
        foreach (var line in lines)
        {
            if (line.Quantite <= 0)
                throw new InvalidOperationException(
                    $"La quantité doit être supérieure à zéro (produit ID {line.ProduitId}).");
        }

        // Load products in one query
        var productIds = lines.Select(l => l.ProduitId).Distinct().ToList();
        var products = await context.Produits
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        if (products.Count != productIds.Count)
            throw new InvalidOperationException("Un ou plusieurs produits sont introuvables.");

        // Validate stock availability at creation time
        foreach (var line in lines)
        {
            var product = products[line.ProduitId];
            if (product.Stock < line.Quantite)
                throw new InvalidOperationException(
                    $"Stock insuffisant pour « {product.Libelle} » " +
                    $"(disponible : {product.Stock}, demandé : {line.Quantite}).");
        }

        // Build lines and compute total HT
        foreach (var line in lines)
        {
            var product = products[line.ProduitId];
            commande.Lignes.Add(new LigneCommande
            {
                ProduitId = product.Id,
                Produit = product,
                Quantite = line.Quantite,
                PrixUnitaire = product.PrixUnitaire,
            });
        }

        commande.Total = commande.Lignes.Sum(l => l.PrixUnitaire * l.Quantite);
    }

    private async Task<string> GenerateNumeroAsync(CancellationToken ct)
    {
        var next = (await context.Commandes.MaxAsync(c => (int?)c.Id, ct) ?? 0) + 1;
        return $"CMD-{DateTime.UtcNow:yyyyMMdd}-{next:0000}";
    }

    private static CommandeDto ToDto(Commande commande)
    {
        var totalHt  = commande.Total;
        var tva      = Math.Round(totalHt * BusinessConstants.TvaRate, 2);
        var totalTtc = totalHt + tva;

        return new CommandeDto
        {
            Id = commande.Id,
            Numero = commande.Numero,
            ClientId = commande.ClientId,
            ClientNom = commande.Client.Nom,
            DateCommande = commande.DateCommande,
            Statut = commande.Statut,
            TotalHt = totalHt,
            Tva = tva,
            TotalTtc = totalTtc,
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
                TotalLigne = l.PrixUnitaire * l.Quantite,
            }).ToList(),
        };
    }
}
