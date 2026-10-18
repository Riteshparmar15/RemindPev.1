using RemindPe.Models;

namespace RemindPe.Repositories;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(int id);
    Task<IEnumerable<Transaction>> GetByCustomerIdAsync(int customerId);
    Task<(List<Transaction> Items, int TotalCount)> GetTransactionsAsync(
        int? customerId = null,
        string? type = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 50);
    Task<Transaction> AddAsync(Transaction transaction);
    Task<Transaction> UpdateAsync(Transaction transaction);
    Task DeleteAsync(Transaction transaction);
}
