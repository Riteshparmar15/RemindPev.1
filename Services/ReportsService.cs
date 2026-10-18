using Microsoft.EntityFrameworkCore;
using RemindPe.Data;
using RemindPe.Models.DTOs;

namespace RemindPe.Services;

public class ReportsService : IReportsService
{
    private readonly KhataDbContext _context;

    public ReportsService(KhataDbContext context)
    {
        _context = context;
    }

    public async Task<LedgerReportSummaryDto> GetSummaryReportAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = endDate ?? DateTime.UtcNow;

        var query = _context.Transactions
            .AsNoTracking()
            .Where(t => t.Date >= start && t.Date <= end);

        var creditGiven = await query
            .Where(t => t.Type == "Credit")
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;

        var debitReceived = await query
            .Where(t => t.Type == "Debit")
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;

        var txCount = await query.CountAsync();

        var activeCustomers = await query
            .Select(t => t.CustomerId)
            .Distinct()
            .CountAsync();

        // Customer balances snapshot
        var balances = await _context.Customers
            .AsNoTracking()
            .Select(c => (c.Transactions.Where(t => t.Type == "Credit").Sum(t => (decimal?)t.Amount) ?? 0m) -
                         (c.Transactions.Where(t => t.Type == "Debit").Sum(t => (decimal?)t.Amount) ?? 0m))
            .ToListAsync();

        decimal totalOutstanding = 0m;
        int debtorsCount = 0;
        int settledCount = 0;
        int advanceCount = 0;

        foreach (var b in balances)
        {
            if (b > 0)
            {
                totalOutstanding += b;
                debtorsCount++;
            }
            else if (b < 0)
            {
                advanceCount++;
            }
            else
            {
                settledCount++;
            }
        }

        return new LedgerReportSummaryDto(
            start,
            end,
            creditGiven,
            debitReceived,
            creditGiven - debitReceived,
            txCount,
            activeCustomers,
            totalOutstanding,
            debtorsCount,
            settledCount,
            advanceCount
        );
    }
}
