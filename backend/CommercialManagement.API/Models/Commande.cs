using CommercialManagement.API.Enums;

namespace CommercialManagement.API.Models;



public class Commande : EntityBase
{
    public string Numero { get; set; } = string.Empty;

    public int ClientId { get; set; }

    public DateTime DateCommande { get; set; } = DateTime.UtcNow;

    public StatutCommande Statut { get; set; }
        = StatutCommande.Brouillon;

    public int TvaId { get; set; }

    public Tva? Tva { get; set; }

    public decimal Total { get; set; }

    public decimal Remise { get; set; }

    public decimal TotalTtc { get; set; }

    public Client? Client { get; set; }

    public ICollection<LigneCommande> Lignes { get; set; }
        = new List<LigneCommande>();

    public void RecalculerTotal()
    {
        Total = Lignes.Sum(l => l.PrixUnitaire * l.Quantite);

        var montantTva = Tva is null
            ? 0
            : Math.Round(Total * Tva.Valeur / 100m, 2);

        TotalTtc = Math.Round(Total + montantTva - Remise, 2);
    }
}