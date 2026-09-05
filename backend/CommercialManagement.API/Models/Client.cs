namespace CommercialManagement.API.Models;

public class Client : EntityBase
{
    public string Identifiant { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string? Prenom { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Telephone { get; set; }
    public string? Adresse { get; set; }
    public string? Ville { get; set; }
    public string? CodePostal { get; set; }

    public ICollection<Commande> Commandes { get; set; } = [];
}
