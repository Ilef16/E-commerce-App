namespace CommercialManagement.API.Models;

public class LigneCommande : EntityBase
{
    public int CommandeId { get; set; }
    public int ProduitId { get; set; }
    public int Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public decimal TotalLigne { get; set; }

    public Commande Commande { get; set; } = null!;
    public Produit Produit { get; set; } = null!;
}
