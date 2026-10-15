using Microsoft.EntityFrameworkCore;
using RemindPe.Data;
using RemindPe.Models;

namespace RemindPe.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly KhataDbContext _context;

    public CustomerRepository(KhataDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Customer>> GetAllWithTransactionsAsync(string? search = null)
    {
        var query = _context.Customers
            .AsNoTracking()
            .Include(c => c.Transactions)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(term) || c.Phone.Contains(term));
        }

        return await query.ToListAsync();
    }

    public async Task<Customer?> GetByIdWithTransactionsAsync(int id)
    {
        return await _context.Customers
            .AsNoTracking()
            .Include(c => c.Transactions)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers.FindAsync(id);
    }

    public async Task<Customer> AddAsync(Customer customer)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer> UpdateAsync(Customer customer)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer> CreateCustomerWithOpeningBalanceAsync(Customer customer, decimal openingAmount, string openingType)
    {
        await using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var tx = new Transaction
            {
                CustomerId = customer.Id,
                Amount = openingAmount,
                Type = openingType,
                Note = "Opening Balance",
                Date = DateTime.UtcNow
            };
            _context.Transactions.Add(tx);
            await _context.SaveChangesAsync();

            await dbTransaction.CommitAsync();

            // Attach created transaction to customer object
            customer.Transactions.Add(tx);
            return customer;
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteAsync(Customer customer)
    {
        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Customers.AnyAsync(c => c.Id == id);
    }

    public async Task<bool> PhoneExistsAsync(string phone, int? excludeId = null)
    {
        var query = _context.Customers.AsNoTracking().Where(c => c.Phone == phone);
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }
        return await query.AnyAsync();
    }

    public async Task<RemindPe.Models.DTOs.DashboardStatsDto> GetDashboardStatsAsync()
    {
        var totalCustomers = await _context.Customers.CountAsync();
        var totalTransactions = await _context.Transactions.CountAsync();
        var totalReceived = await _context.Transactions
            .Where(t => t.Type == "Debit")
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;

        // Perform balance projection at database level to avoid loading millions of entity objects into RAM
        var balances = await _context.Customers
            .AsNoTracking()
            .Select(c => (c.Transactions.Where(t => t.Type == "Credit").Sum(t => (decimal?)t.Amount) ?? 0m) -
                         (c.Transactions.Where(t => t.Type == "Debit").Sum(t => (decimal?)t.Amount) ?? 0m))
            .ToListAsync();

        decimal totalReceivable = 0m;
        int pendingCustomersCount = 0;

        foreach (var b in balances)
        {
            if (b > 0)
            {
                totalReceivable += b;
                pendingCustomersCount++;
            }
        }

        return new RemindPe.Models.DTOs.DashboardStatsDto(
            totalReceivable,
            totalReceived,
            totalCustomers,
            pendingCustomersCount,
            totalTransactions
        );
    }
}
