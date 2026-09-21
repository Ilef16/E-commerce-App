using CommercialManagement.API.Enums;

namespace CommercialManagement.API.Models;

public class Tva : EntityBase
{
    public string Libelle { get; set; } = string.Empty;

    public int Type { get; set; }

    public decimal Valeur { get; set; }

    public bool Etat { get; set; } = true;

    public ICollection<Commande> Commandes { get; set; }
        = new List<Commande>();
}
