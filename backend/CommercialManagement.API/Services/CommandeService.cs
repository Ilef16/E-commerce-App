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
    public async Task<PagedResult<CommandeDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = BuildQuery().OrderByDescending(c => c.DateCommande);
        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<CommandeDto>
        {
            Items      = items.Select(ToDto).ToList(),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize,
        };
    }

    public async Task<CommandeDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var commande = await BuildQuery().FirstOrDefaultAsync(c => c.Id == id, ct);
        return commande is null ? null : ToDto(commande);
    }

    public async Task<CommandeDto> CreateAsync(CommandeWriteDto input,TvaWriteDto input1,CancellationToken ct = default ) 
    {
        var commande = new Commande
        {
            Numero       = await GenerateNumeroAsync(ct),
            ClientId     = input.ClientId,
            DateCommande = input.DateCommande ?? DateTime.UtcNow,
        };
        var tva = new Tva
        {
            Libelle = input1.Libelle,
            Type = input1.Type,
            Valeur= input1.Valeur,
            Etat = input1.Etat
        };

        if (tva.Type < 0)
            throw new InvalidOperationException("type ne peut pas etre inférieur a 0");

    
        
        await ApplyLinesAsync(commande, input.Lignes ,ct);

        context.Commandes.Add(commande);
        //context.tvas.Add(tva);
        await context.SaveChangesAsync(ct);

        return ToDto(await BuildQuery().FirstAsync(c => c.Id == commande.Id, ct));
    }

    public async Task<CommandeDto?> UpdateAsync(int id, CommandeWriteDto input, CancellationToken ct = default)
    {
        var commande = await BuildQuery().FirstOrDefaultAsync(c => c.Id == id, ct);
        if (commande is null) return null;

        if (commande.Statut != StatutCommande.Brouillon)
            throw new InvalidOperationException("Seule une commande en statut Brouillon peut être modifiée.");

        commande.ClientId     = input.ClientId;
        commande.DateCommande = input.DateCommande ?? commande.DateCommande;

        context.LignesCommande.RemoveRange(commande.Lignes);
        commande.Lignes.Clear();

        await ApplyLinesAsync(commande, input.Lignes, ct);

        commande.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);

        return ToDto(await BuildQuery().FirstAsync(c => c.Id == id, ct));
    }

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

    public async Task<CommandeDto?> ValidateAsync(int id, CancellationToken ct = default)
    {
        var commande = await BuildQuery().FirstOrDefaultAsync(c => c.Id == id, ct);
        if (commande is null) return null;

        if (commande.Statut != StatutCommande.Brouillon)
            throw new InvalidOperationException("Cette commande est déjà validée ou annulée.");

        if (commande.Lignes.Count == 0)
            throw new InvalidOperationException("Une commande doit contenir au moins une ligne.");

        foreach (var ligne in commande.Lignes)
        {
            if (ligne.Produit.Stock < ligne.Quantite)
                throw new InvalidOperationException(
                    $"Stock insuffisant pour « {ligne.Produit.Libelle} » " +
                    $"(disponible : {ligne.Produit.Stock}, demandé : {ligne.Quantite}).");
        }


        foreach (var ligne in commande.Lignes)
            ligne.Produit.Stock -= ligne.Quantite;

        commande.Statut    = StatutCommande.Validee;
        commande.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);

        return ToDto(await BuildQuery().FirstAsync(c => c.Id == id, ct));
    }

    public async Task<CommandeDto?> CancelAsync(int id, CancellationToken ct = default)
    {
        var commande = await BuildQuery().FirstOrDefaultAsync(c => c.Id == id, ct);
        if (commande is null) return null;

        if (commande.Statut == StatutCommande.Annulee)
            throw new InvalidOperationException("Cette commande est déjà annulée.");

        if (commande.Statut == StatutCommande.Livree)
            throw new InvalidOperationException("Une commande livrée ne peut pas être annulée.");

        if (commande.Statut is not (StatutCommande.Brouillon or StatutCommande.Validee))
            throw new InvalidOperationException("Cette commande ne peut pas être annulée.");

        if (commande.Remise > commande.Total)
        {
            throw new InvalidOperationException("Remise ne doit pas etre supérieur au total");
        }
        if (commande.Remise<0)
        {
            throw new InvalidOperationException("Remise doit etre positive");
        }

        await using var transaction = await context.Database.BeginTransactionAsync(ct);

        if (commande.Statut == StatutCommande.Validee)
        {
            foreach (var ligne in commande.Lignes)
            {
                var quantite = ligne.Quantite;
                await context.Produits
                    .Where(p => p.Id == ligne.ProduitId)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.Stock, p => p.Stock + quantite), ct);
            }
        }
       /* if(commande.Remise < 0)
        {
            var re = commande.Remise;
            re.CompareTo(commande.Total);
            await context.tva
            .where(re => re.Id == commande.Id)
            .ExecuteUpdateAsync()
        }*/
        await context.Commandes
            .Where(c => c.Id == id)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(c => c.Statut, StatutCommande.Annulee)
                    .SetProperty(c => c.UpdatedAt, DateTime.UtcNow),
                ct);

        await transaction.CommitAsync(ct);
        context.ChangeTracker.Clear();

        return ToDto(await BuildQuery().FirstAsync(c => c.Id == id, ct));
    }

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

        if (!await context.Clients.AnyAsync(c => c.Id == commande.ClientId, ct))
            throw new InvalidOperationException("Le client indiqué est introuvable.");


        /* 
        ici des controles pour le tax , remise 
        abscence de tax
        abscence de remise 
        des valeurs invalides
        */

        var productIds = lines.Select(l => l.ProduitId).Distinct().ToList();
        var products = await context.Produits
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        if (products.Count != productIds.Count)
            throw new InvalidOperationException("Un ou plusieurs produits sont introuvables.");

        foreach (var line in lines)
        {
            if (line.Quantite <= 0)
                throw new InvalidOperationException(
                    $"La quantité doit être supérieure à zéro (produit ID {line.ProduitId}).");

            var product = products[line.ProduitId];
            if (product.Stock < line.Quantite)
                throw new InvalidOperationException(
                    $"Stock insuffisant pour « {product.Libelle} » " +
                    $"(disponible : {product.Stock}, demandé : {line.Quantite}).");


            commande.Lignes.Add(new LigneCommande
            {
                ProduitId    = product.Id,
                Produit      = product,
                Quantite     = line.Quantite,
                PrixUnitaire = product.PrixUnitaire,
            }

           
            );
        }

        commande.RecalculerTotal();
    }

    private async Task<string> GenerateNumeroAsync(CancellationToken ct)
    {
        var nextId = (await context.Commandes.MaxAsync(c => (int?)c.Id, ct) ?? 0) + 1;
        return $"CMD-{DateTime.UtcNow:yyyyMMdd}-{nextId:0000}";
    }

    
 /*   public async Task<TvaDto> CreateTVA(TvaWriteDto input, CancellationToken ct = default)
    {
        var libelle = input.Libelle.Trim();
       

        var Tva = new Tva();
        context.tvas.Add(Tva);
        await context.SaveChangesAsync(ct);
    }*/
   /* private static void TvaDto (Tva tva , TvaWriteDto tvaWriteDto)
    {
       return new TvaDto
       {
           tva.Libelle = tvaWriteDto.Libelle,
           tva.Type = tva.Type,
           tva.Valeur = tvaWriteDto.Valeur,
           tva.Etat = tvaWriteDto.Etat,
       };*/
       /* return new TvaDto
        {
            Libelle= tva.Libelle,
            Type= tva.Type,
            Valeur= tva.valeur,
            Etat= tva.Etat,
        };*/
    
    private static CommandeDto ToDto(Commande commande)
    {   
        var totalHt  = commande.Total;
        // var tva      = Math.Round(totalHt * BusinessConstants.TvaRate, 2);
        var Remise = commande.Remise;


        return new CommandeDto
        {
            Id           = commande.Id,
            Numero       = commande.Numero,
            ClientId     = commande.ClientId,
            ClientNom    = commande.Client.Nom,
            DateCommande = commande.DateCommande,
            Statut       = commande.Statut,
            TotalHt      = totalHt,
            // Tva          = tva,
            // TotalTtc     = totalHt + tva,
            
            Remise       = commande.Remise,
            CreatedAt    = commande.CreatedAt,
            UpdatedAt    = commande.UpdatedAt,
            Lignes       = commande.Lignes.Select(l => new CommandeLigneDto
            {
                Id               = l.Id,
                ProduitId        = l.ProduitId,
                ReferenceProduit = l.Produit.Reference,
                LibelleProduit   = l.Produit.Libelle,
                Quantite         = l.Quantite,
                PrixUnitaire     = l.PrixUnitaire,
                TotalLigne       = l.PrixUnitaire * l.Quantite,
            }).ToList(),
             
        };
    }
}
