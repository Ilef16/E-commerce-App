using CommercialManagement.API.DTOs;

namespace CommercialManagement.API.Interfaces;

public interface ICommandeService
{
    Task<IReadOnlyList<CommandeDto>> GetAllAsync(CancellationToken ct = default);
    Task<CommandeDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<CommandeDto> CreateAsync(CommandeWriteDto input, CancellationToken ct = default);
    Task<CommandeDto?> UpdateAsync(int id, CommandeWriteDto input, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<CommandeDto?> ValidateAsync(int id, CancellationToken ct = default);
}
