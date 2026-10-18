using Microsoft.EntityFrameworkCore;
using RemindPe.Data;
using RemindPe.Models;
using RemindPe.Services;
using Xunit;

namespace RemindPe.Tests;

public class ReportsServiceTests
{
    private KhataDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<KhataDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new KhataDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task GetSummaryReportAsync_AggregatesTotalsAccurately()
    {
        using var context = CreateInMemoryDbContext();

        // Customer 1: 1500 Credit, 500 Debit -> Net 1000 Due
        var c1 = new Customer { Id = 1, Name = "Aman", Phone = "9876543210" };
        // Customer 2: 800 Credit, 1000 Debit -> Net -200 Advance
        var c2 = new Customer { Id = 2, Name = "Bina", Phone = "9876543211" };
        context.Customers.AddRange(c1, c2);

        var now = DateTime.UtcNow;
        context.Transactions.AddRange(
            new Transaction { CustomerId = 1, Amount = 1500m, Type = "Credit", Date = now.AddDays(-2) },
            new Transaction { CustomerId = 1, Amount = 500m, Type = "Debit", Date = now.AddDays(-1) },
            new Transaction { CustomerId = 2, Amount = 800m, Type = "Credit", Date = now.AddDays(-3) },
            new Transaction { CustomerId = 2, Amount = 1000m, Type = "Debit", Date = now.AddDays(-1) }
        );
        await context.SaveChangesAsync();

        var reportsService = new ReportsService(context);

        var report = await reportsService.GetSummaryReportAsync(now.AddDays(-7), now.AddDays(1));

        Assert.Equal(2300m, report.TotalCreditGiven); // 1500 + 800
        Assert.Equal(1500m, report.TotalPaymentsReceived); // 500 + 1000
        Assert.Equal(800m, report.NetChange); // 2300 - 1500
        Assert.Equal(4, report.TransactionCount);
        Assert.Equal(2, report.ActiveCustomersCount);
        Assert.Equal(1000m, report.TotalOutstandingReceivables); // Customer 1 only
        Assert.Equal(1, report.DebtorsCount); // Customer 1
        Assert.Equal(1, report.AdvanceCount); // Customer 2
        Assert.Equal(0, report.SettledCount);
    }
}
