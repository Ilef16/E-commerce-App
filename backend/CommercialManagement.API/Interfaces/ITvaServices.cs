using CommercialManagement.API.DTOs;

namespace CommercialManagement.API.Interfaces;

public interface ItvaServices
{
   
    Task<CommandeDto> CreateAsync( TvaWriteDto input1, CancellationToken ct = default);
    
}
