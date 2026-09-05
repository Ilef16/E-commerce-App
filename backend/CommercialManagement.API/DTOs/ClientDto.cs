namespace CommercialManagement.API.DTOs;

public class ClientWriteDto
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(30)]
    public string Identifiant { get; init; } = string.Empty;
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MaxLength(120)]
    public string Nom { get; init; } = string.Empty;
    public string? Prenom { get; init; }
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    [System.ComponentModel.DataAnnotations.MaxLength(180)]
    public string Email { get; init; } = string.Empty;
    public string? Telephone { get; init; }
    public string? Adresse { get; init; }
    public string? Ville { get; init; }
    public string? CodePostal { get; init; }
}

public class ClientDto
{
    public int Id { get; init; }
    public string Nom { get; init; } = string.Empty;
    public string? Prenom { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? Telephone { get; init; }
    public string? Adresse { get; init; }
    public string? Ville { get; init; }
    public string? CodePostal { get; init; }
    public string Identifiant { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public int NombreCommandes { get; init; }
}
