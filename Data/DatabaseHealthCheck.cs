using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace RemindPe.Data;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly KhataDbContext _context;

    public DatabaseHealthCheck(KhataDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy("Cannot connect to SQLite database.");
            }

            var customerCount = await _context.Customers.CountAsync(cancellationToken);
            return HealthCheckResult.Healthy($"SQLite database is online. {customerCount} customer records.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("SQLite database is unreachable.", ex);
        }
    }
}
