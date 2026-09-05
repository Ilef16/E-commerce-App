using CommercialManagement.API.DTOs;

namespace CommercialManagement.API.Interfaces;

public interface IClientService
{
    Task<PagedResult<ClientDto>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken ct = default);
    Task<ClientDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ClientDto> CreateAsync(ClientWriteDto input, CancellationToken ct = default);
    Task<ClientDto?> UpdateAsync(int id, ClientWriteDto input, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
