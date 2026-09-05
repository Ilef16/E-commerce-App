using CommercialManagement.API.Data;
using CommercialManagement.API.DTOs;
using CommercialManagement.API.Interfaces;
using CommercialManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CommercialManagement.API.Services;

public class ProduitService(ApplicationDbContext context) : IProduitService
{
    public async Task<IReadOnlyList<ProduitDto>> GetAllAsync(CancellationToken ct = default)
    {
        var produits = await context.Produits.AsNoTracking().OrderBy(p => p.Libelle).ToListAsync(ct);
        return produits.Select(ToDto).ToList();
    }

    public async Task<ProduitDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var produit = await context.Produits.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        return produit is null ? null : ToDto(produit);
    }

    public async Task<ProduitDto> CreateAsync(ProduitWriteDto input, CancellationToken ct = default)
    {
        var reference = input.Reference.Trim();
        if (await context.Produits.AnyAsync(p => p.Reference == reference, ct))
            throw new InvalidOperationException("Un produit avec cette référence existe déjà.");

        var produit = new Produit();
        ApplyInput(produit, input);
        context.Produits.Add(produit);
        await context.SaveChangesAsync(ct);
        return ToDto(produit);
    }

    public async Task<ProduitDto?> UpdateAsync(int id, ProduitWriteDto input, CancellationToken ct = default)
    {
        var produit = await context.Produits.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (produit is null) return null;

        var reference = input.Reference.Trim();
        if (await context.Produits.AnyAsync(p => p.Id != id && p.Reference == reference, ct))
            throw new InvalidOperationException("Un produit avec cette référence existe déjà.");

        ApplyInput(produit, input);
        produit.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return ToDto(produit);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var produit = await context.Produits.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (produit is null) return false;
        context.Produits.Remove(produit);
        await context.SaveChangesAsync(ct);
        return true;
    }

    private static void ApplyInput(Produit produit, ProduitWriteDto input)
    {
        produit.Reference = input.Reference.Trim();
        produit.Libelle = input.Libelle.Trim();
        produit.Description = input.Description?.Trim();
        produit.PrixUnitaire = input.PrixUnitaire;
        produit.Stock = input.Stock;
    }

    private static ProduitDto ToDto(Produit produit) => new()
    {
        Id = produit.Id,
        Reference = produit.Reference,
        Libelle = produit.Libelle,
        Description = produit.Description,
        PrixUnitaire = produit.PrixUnitaire,
        Stock = produit.Stock,
        CreatedAt = produit.CreatedAt,
        UpdatedAt = produit.UpdatedAt
    };
}
