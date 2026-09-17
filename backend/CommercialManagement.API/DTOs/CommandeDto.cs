using System.ComponentModel.DataAnnotations;
using CommercialManagement.API.Enums;

namespace CommercialManagement.API.DTOs;

public class CommandeLigneWriteDto
{
    [Range(1, int.MaxValue)]
    public int ProduitId { get; init; }

    [Range(1, int.MaxValue)]
    public int Quantite { get; init; }
}

public class CommandeWriteDto
{
    [Range(1, int.MaxValue)]
    public int ClientId { get; init; }

    public DateTime? DateCommande { get; init; }

    [Required, MinLength(1)]
    public List<CommandeLigneWriteDto> Lignes { get; init; } = [];
}

public class CommandeLigneDto
{
    public int Id { get; init; }
    public int ProduitId { get; init; }
    public string ReferenceProduit { get; init; } = string.Empty;
    public string LibelleProduit { get; init; } = string.Empty;
    public int Quantite { get; init; }
    public decimal PrixUnitaire { get; init; }
    public decimal TotalLigne { get; init; }
}

public class CommandeDto
{
    public int Id { get; init; }
    public string Numero { get; init; } = string.Empty;
    public int ClientId { get; init; }
    public string ClientNom { get; init; } = string.Empty;
    public DateTime DateCommande { get; init; }
    public StatutCommande Statut { get; init; }
    public decimal TotalHt { get; init; }
    public decimal Tva { get; init; }
    public decimal TotalTtc { get; init; }
    public decimal Remise { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public List<CommandeLigneDto> Lignes { get; init; } = [];
    public List<TvaDto> Tvas { get; init;} = new List<TvaDto>();
}
