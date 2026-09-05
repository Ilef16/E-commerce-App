using CommercialManagement.API.Data;
using CommercialManagement.API.DTOs;
using CommercialManagement.API.Interfaces;
using CommercialManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CommercialManagement.API.Services;

public class ClientService(ApplicationDbContext context, ILogger<ClientService> logger) : IClientService
{
    public async Task<ClientDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var client = await context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
        return client is null ? null : ToDto(client);
    }

    public async Task<ClientDto> CreateAsync(ClientWriteDto input, CancellationToken ct = default)
    {
        var identifier = input.Identifiant.Trim();
        var identifierExists = await context.Clients.AnyAsync(c => c.Identifiant == identifier, ct);
        if (identifierExists)
            throw new InvalidOperationException("Un client avec cet identifiant existe déjà.");

        var email = input.Email.Trim();
        var emailExists = await context.Clients.AnyAsync(c => c.Email == email, ct);
        if (emailExists)
            throw new InvalidOperationException("Un client avec cet email existe déjà.");

        var client = new Client();
        ApplyInput(client, input);
        context.Clients.Add(client);
        await context.SaveChangesAsync(ct);
        return ToDto(client);
    }

    public async Task<ClientDto?> UpdateAsync(int id, ClientWriteDto input, CancellationToken ct = default)
    {
        var client = await context.Clients.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (client is null) return null;

        var email = input.Email.Trim();
        var emailExists = await context.Clients.AnyAsync(c => c.Id != id && c.Email == email, ct);
        if (emailExists)
            throw new InvalidOperationException("Un client avec cet email existe déjà.");

        var identifier = input.Identifiant.Trim();
        var identifierExists = await context.Clients.AnyAsync(c => c.Id != id && c.Identifiant == identifier, ct);
        if (identifierExists)
            throw new InvalidOperationException("Un client avec cet identifiant existe déjà.");

        ApplyInput(client, input);
        client.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return ToDto(client);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var client = await context.Clients.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (client is null) return false;

        context.Clients.Remove(client);
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<PagedResult<ClientDto>> GetPagedAsync(int page, int pageSize, string? q, CancellationToken ct = default)
    {
        var query = context.Clients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(c =>
                c.Nom.ToLower().Contains(term) ||
                (c.Prenom != null && c.Prenom.ToLower().Contains(term)) ||
                c.Email.ToLower().Contains(term));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(c => c.Nom)
            .ThenBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => ToDto(c))
            .ToListAsync(ct);

        logger.LogInformation("Clients listed — page {Page}/{PageSize}, total {Total}", page, pageSize, total);

        return new PagedResult<ClientDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    private static ClientDto ToDto(Client c) => new()
    {
        Id = c.Id,
        Nom = c.Nom,
        Prenom = c.Prenom,
        Email = c.Email,
        Telephone = c.Telephone,
        Adresse = c.Adresse,
        Ville = c.Ville,
        CodePostal = c.CodePostal,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
        NombreCommandes = 0,
        Identifiant = c.Identifiant,
    };

    private static void ApplyInput(Client client, ClientWriteDto input)
    {
        client.Nom = input.Nom.Trim();
        client.Prenom = input.Prenom?.Trim();
        client.Email = input.Email.Trim();
        client.Telephone = input.Telephone?.Trim();
        client.Adresse = input.Adresse?.Trim();
        client.Ville = input.Ville?.Trim();
        client.CodePostal = input.CodePostal?.Trim();
        client.Identifiant = input.Identifiant.Trim();
    }
}
