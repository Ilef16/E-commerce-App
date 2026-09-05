namespace CommercialManagement.API.Models;

public class Produit : EntityBase
{
    public string Reference { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal PrixUnitaire { get; set; }
    public int Stock { get; set; }

    public ICollection<LigneCommande> LignesCommande { get; set; } = [];
}
