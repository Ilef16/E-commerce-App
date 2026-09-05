using CommercialManagement.API.Constants;
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

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => ToDto(p))
            .ToListAsync(ct);

        return new PagedResult<ProduitDto>
        {
            Items      = items,
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize,
        };
    }

    public async Task<ProduitDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var produit = await context.Produits.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        return produit is null ? null : ToDto(produit);
    }

    public async Task<ProduitDto> CreateAsync(ProduitWriteDto input, CancellationToken ct = default)
    {
        var reference = string.IsNullOrWhiteSpace(input.Reference)
            ? await GenerateReferenceAsync(ct)
            : input.Reference.Trim();

        if (await context.Produits.AnyAsync(p => p.Reference == reference, ct))
            throw new InvalidOperationException("Un produit avec cette référence existe déjà.");

        var produit = new Produit();
        ApplyInput(produit, input);
        produit.Reference = reference;
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

        if (await context.LignesCommande.AnyAsync(ligne => ligne.ProduitId == id, ct))
            throw new InvalidOperationException("Ce produit est utilisé par une commande et ne peut pas être supprimé.");

        context.Produits.Remove(produit);
        await context.SaveChangesAsync(ct);
        return true;
    }

    private static void ApplyInput(Produit produit, ProduitWriteDto input)
    {
        produit.Reference    = input.Reference.Trim();
        produit.Libelle      = input.Libelle.Trim();
        produit.Description  = input.Description?.Trim();
        produit.PrixUnitaire = input.PrixUnitaire;
        produit.Stock        = input.Stock;
    }

    private static ProduitDto ToDto(Produit produit) => new()
    {
        Id           = produit.Id,
        Reference    = produit.Reference,
        Libelle      = produit.Libelle,
        Description  = produit.Description,
        PhotoUrl     = produit.PhotoUrl,
        PrixUnitaire = produit.PrixUnitaire,
        Stock        = produit.Stock,
        CreatedAt    = produit.CreatedAt,
        UpdatedAt    = produit.UpdatedAt,
    };

    private async Task<string> GenerateReferenceAsync(CancellationToken ct)
    {
        var nextId = (await context.Produits.MaxAsync(p => (int?)p.Id, ct) ?? 0) + 1;
        return $"PROD-{DateTime.UtcNow:yyyyMMdd}-{nextId:0000}";
    }

    private async Task<string?> SavePhotoAsync(IFormFile? photo, CancellationToken ct)
    {
        if (photo is null || photo.Length == 0)
            return null;

        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();

        if (!BusinessConstants.AllowedPhotoExtensions.Contains(extension))
            throw new InvalidOperationException(
                $"Format d'image non supporté. Extensions acceptées : {string.Join(", ", BusinessConstants.AllowedPhotoExtensions)}.");

        var uploadDir = Path.Combine(
            environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"),
            BusinessConstants.ProductUploadFolder);

        Directory.CreateDirectory(uploadDir);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadDir, fileName);

        await using var stream = File.Create(filePath);
        await photo.CopyToAsync(stream, ct);

        return $"/{BusinessConstants.ProductUploadFolder}/{fileName}";
    }
}
