using CommercialManagement.API.DTOs;

namespace CommercialManagement.API.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetStatsAsync(CancellationToken ct = default);
}
