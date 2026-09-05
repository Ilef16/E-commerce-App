using CommercialManagement.API.Data;
using CommercialManagement.API.DTOs;
using CommercialManagement.API.Interfaces;
using CommercialManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CommercialManagement.API.Services;

public class ProduitService(ApplicationDbContext context, IWebHostEnvironment environment) : IProduitService
{
    public async Task<PagedResult<ProduitDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = context.Produits.AsNoTracking().OrderBy(p => p.Libelle);
        var total = await query.CountAsync(ct);
        var produits = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<ProduitDto> { Items = produits.Select(ToDto).ToList(), TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<ProduitDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var produit = await context.Produits.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        return produit is null ? null : ToDto(produit);
    }

    public async Task<ProduitDto> CreateAsync(ProduitWriteDto input, CancellationToken ct = default)
    {
        var reference = await GenerateReferenceAsync(ct);
        if (await context.Produits.AnyAsync(p => p.Reference == reference, ct))
            throw new InvalidOperationException("Un produit avec cette référence existe déjà.");

        var produit = new Produit();
        ApplyInput(produit, input);
        produit.PhotoUrl = await SavePhotoAsync(input.Photo, ct);
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
        if (input.RemovePhoto)
            produit.PhotoUrl = null;
        else if (input.Photo is not null)
            produit.PhotoUrl = await SavePhotoAsync(input.Photo, ct);
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

    /// <summary>Maps writable DTO fields onto the produit entity. Photo URL is managed separately.</summary>
    private static void ApplyInput(Produit produit, ProduitWriteDto input)
    {
        produit.Reference    = input.Reference.Trim();
        produit.Libelle      = input.Libelle.Trim();
        produit.Description  = input.Description?.Trim();
        produit.PrixUnitaire = input.PrixUnitaire;
        produit.Stock        = input.Stock;
        // PhotoUrl is intentionally excluded — managed via SavePhotoAsync / RemovePhoto logic
    }

    private static ProduitDto ToDto(Produit produit) => new()
    {
        Id = produit.Id,
        Reference = produit.Reference,
        Libelle = produit.Libelle,
        Description = produit.Description,
        PhotoUrl = produit.PhotoUrl,
        PrixUnitaire = produit.PrixUnitaire,
        Stock = produit.Stock,
        CreatedAt = produit.CreatedAt,
        UpdatedAt = produit.UpdatedAt
    };

    private async Task<string> GenerateReferenceAsync(CancellationToken ct)
    {
        var next = (await context.Produits.MaxAsync(p => (int?)p.Id, ct) ?? 0) + 1;
        return $"PROD-{DateTime.UtcNow:yyyyMMdd}-{next:0000}";
    }

    private async Task<string?> SavePhotoAsync(IFormFile? photo, CancellationToken ct)
    {
        if (photo is null || photo.Length == 0) return null;
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp", ".avif" };
        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException("Format d’image non supporté. Utilisez JPG, PNG, WEBP, GIF, BMP ou AVIF.");
        var directory = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "uploads", "products");
        Directory.CreateDirectory(directory);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        await using var stream = File.Create(Path.Combine(directory, fileName));
        await photo.CopyToAsync(stream, ct);
        return $"/uploads/products/{fileName}";
    }
}
