using System.ComponentModel.DataAnnotations;
using CommercialManagement.API.Enums;

namespace CommercialManagement.API.DTOs;
public class TvaWriteDto
{
    [Required]
    public string Libelle { get; init; } = "";
    public int Type { get; init;}
    public decimal Valeur { get; init; }
    public bool Etat { get; init; } = false;

}
public class TvaDto
{
    public int Libelle { get; init; }
    public string Type { get; init; } = string.Empty;
    public decimal Valeur { get; init;}

    public bool Etat { get;}
}
