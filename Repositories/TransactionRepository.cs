using Microsoft.EntityFrameworkCore;
using RemindPe.Data;
using RemindPe.Models;

namespace RemindPe.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly KhataDbContext _context;

    public TransactionRepository(KhataDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction?> GetByIdAsync(int id)
    {
        return await _context.Transactions
            .Include(t => t.Customer)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Transaction>> GetByCustomerIdAsync(int customerId)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Where(t => t.CustomerId == customerId)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<(List<Transaction> Items, int TotalCount)> GetTransactionsAsync(
        int? customerId = null,
        string? type = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 50)
    {
        var query = _context.Transactions
            .AsNoTracking()
            .Include(t => t.Customer)
            .AsQueryable();

        if (customerId.HasValue && customerId.Value > 0)
        {
            query = query.Where(t => t.CustomerId == customerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            var normalizedType = type.Trim();
            query = query.Where(t => t.Type == normalizedType);
        }

        if (startDate.HasValue)
        {
            query = query.Where(t => t.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.Date <= endDate.Value);
        }

        var totalCount = await query.CountAsync();

        // Enforce safe bounds on pagination
        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 500);

        var items = await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Transaction> AddAsync(Transaction transaction)
    {
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<Transaction> UpdateAsync(Transaction transaction)
    {
        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task DeleteAsync(Transaction transaction)
    {
        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();
    }
}
