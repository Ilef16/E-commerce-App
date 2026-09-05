using CommercialManagement.API.Constants;
using CommercialManagement.API.Data;
using CommercialManagement.API.DTOs;
using CommercialManagement.API.Enums;
using CommercialManagement.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommercialManagement.API.Services;

public class DashboardService(ApplicationDbContext context) : IDashboardService
{
    public async Task<DashboardDto> GetStatsAsync(CancellationToken ct = default)
    {
        var totalClients   = await context.Clients.CountAsync(ct);
        var totalProduits  = await context.Produits.CountAsync(ct);
        var totalCommandes = await context.Commandes.CountAsync(ct);

        var brouillon = await context.Commandes.CountAsync(c => c.Statut == StatutCommande.Brouillon, ct);
        var validees  = await context.Commandes.CountAsync(c => c.Statut == StatutCommande.Validee,  ct);
        var livrees   = await context.Commandes.CountAsync(c => c.Statut == StatutCommande.Livree,   ct);
        var annulees  = await context.Commandes.CountAsync(c => c.Statut == StatutCommande.Annulee,  ct);

        var caHt = await context.Commandes
            .Where(c => c.Statut == StatutCommande.Validee || c.Statut == StatutCommande.Livree)
            .SumAsync(c => c.Total, ct);

        var stockFaible = await context.Produits
            .CountAsync(p => p.Stock > 0 && p.Stock <= BusinessConstants.StockFaibleSeuil, ct);
        var rupture = await context.Produits.CountAsync(p => p.Stock == 0, ct);

        var tva = Math.Round(caHt * BusinessConstants.TvaRate, 2);

        return new DashboardDto
        {
            TotalClients        = totalClients,
            TotalProduits       = totalProduits,
            TotalCommandes      = totalCommandes,
            CommandesBrouillon  = brouillon,
            CommandesValidees   = validees,
            CommandesLivrees    = livrees,
            CommandesAnnulees   = annulees,
            ChiffreAffairesHt   = caHt,
            ChiffreAffairesTtc  = caHt + tva,
            ProduitsStockFaible = stockFaible,
            ProduitsRupture     = rupture,
        };
    }
}
