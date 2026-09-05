using CommercialManagement.API.DTOs;

namespace CommercialManagement.API.Interfaces;

public interface IProduitService
{
    Task<PagedResult<ProduitDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<ProduitDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ProduitDto> CreateAsync(ProduitWriteDto input, CancellationToken ct = default);
    Task<ProduitDto?> UpdateAsync(int id, ProduitWriteDto input, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
