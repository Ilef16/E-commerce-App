using CommercialManagement.API.Enums;

namespace CommercialManagement.API.Models;

public class Commande : EntityBase
{
    public string Numero { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public DateTime DateCommande { get; set; } = DateTime.UtcNow;
    public StatutCommande Statut { get; set; } = StatutCommande.Brouillon;
    public decimal Total { get; set; }

    public decimal Remise { get; set; } 

    public Client Client { get; set; } = null!;
    public ICollection<LigneCommande> Lignes { get; set; } = [];

    public void RecalculerTotal()
    {
        Total = Lignes.Sum(ligne => ligne.PrixUnitaire * ligne.Quantite);
    }
}
