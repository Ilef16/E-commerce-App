using CommercialManagement.API.DTOs;

namespace CommercialManagement.API.Interfaces;

public interface IProduitService
{
    Task<IReadOnlyList<ProduitDto>> GetAllAsync(CancellationToken ct = default);
    Task<ProduitDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ProduitDto> CreateAsync(ProduitWriteDto input, CancellationToken ct = default);
    Task<ProduitDto?> UpdateAsync(int id, ProduitWriteDto input, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
