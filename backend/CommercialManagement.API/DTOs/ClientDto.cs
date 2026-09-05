using System.ComponentModel.DataAnnotations;

namespace CommercialManagement.API.DTOs;

/// <summary>Payload for creating or updating a client.</summary>
public class ClientWriteDto
{
    [Required, MaxLength(30)]
    public string Identifiant { get; init; } = string.Empty;

    [Required, MaxLength(120)]
    public string Nom { get; init; } = string.Empty;

    [MaxLength(80)]
    public string? Prenom { get; init; }

    [Required, EmailAddress, MaxLength(180)]
    public string Email { get; init; } = string.Empty;

    [MaxLength(30)]
    public string? Telephone { get; init; }

    [MaxLength(250)]
    public string? Adresse { get; init; }

    [MaxLength(100)]
    public string? Ville { get; init; }

    [MaxLength(20)]
    public string? CodePostal { get; init; }
}

/// <summary>Read model returned by the API for a client.</summary>
public class ClientDto
{
    public int Id { get; init; }
    public string Identifiant { get; init; } = string.Empty;
    public string Nom { get; init; } = string.Empty;
    public string? Prenom { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? Telephone { get; init; }
    public string? Adresse { get; init; }
    public string? Ville { get; init; }
    public string? CodePostal { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public int NombreCommandes { get; init; }
}
