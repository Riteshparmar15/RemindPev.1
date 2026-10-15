using Microsoft.Extensions.Caching.Memory;
using RemindPe.Models.DTOs;
using RemindPe.Repositories;

namespace RemindPe.Services;

public class DashboardService : IDashboardService
{
    private const string StatsCacheKey = "remindpe_dashboard_stats_cache";
    private readonly ICustomerRepository _customerRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        ICustomerRepository customerRepository,
        IMemoryCache memoryCache,
        ILogger<DashboardService> logger)
    {
        _customerRepository = customerRepository;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        if (_memoryCache.TryGetValue(StatsCacheKey, out DashboardStatsDto? cached) && cached != null)
        {
            return cached;
        }

        var stats = await _customerRepository.GetDashboardStatsAsync();
        
        // Cache stats for 5 seconds to absorb burst concurrent queries
        _memoryCache.Set(StatsCacheKey, stats, TimeSpan.FromSeconds(5));
        
        return stats;
    }
}
