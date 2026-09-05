using System.ComponentModel.DataAnnotations;

namespace CommercialManagement.API.DTOs;

public class ProduitWriteDto
{
    [MaxLength(40)]
    public string Reference { get; init; } = string.Empty;

    [Required, MaxLength(160)]
    public string Libelle { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }

    public IFormFile? Photo { get; init; }

    public bool RemovePhoto { get; init; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal PrixUnitaire { get; init; }

    [Range(0, int.MaxValue)]
    public int Stock { get; init; }
}

public class ProduitDto
{
    public int Id { get; init; }
    public string Reference { get; init; } = string.Empty;
    public string Libelle { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? PhotoUrl { get; init; }
    public decimal PrixUnitaire { get; init; }
    public int Stock { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
