using System.ComponentModel.DataAnnotations;
using CommercialManagement.API.Enums;

namespace CommercialManagement.API.DTOs;
public class TvaWriteDto
{
    [Required]
    [MaxLength(100)]
    public string Libelle { get; init; } = string.Empty;

    [Range(1, 2)]
    public int Type { get; init; }

    [Range(0, 100)]
    public decimal Valeur { get; init; }

    public bool Etat { get; init; } = true;
}

public class TvaDto
{
    public int Id { get; init; }

    public string Libelle { get; init; } = string.Empty;

    public int Type { get; init; }

    public decimal Valeur { get; init; }

    public bool Etat { get; init; }
}