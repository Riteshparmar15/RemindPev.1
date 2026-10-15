using RemindPe.Models.DTOs;

namespace RemindPe.Services;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}
