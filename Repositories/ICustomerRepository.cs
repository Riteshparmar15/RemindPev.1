using RemindPe.Models;

namespace RemindPe.Repositories;

public interface ICustomerRepository
{
    Task<IEnumerable<Customer>> GetAllWithTransactionsAsync(string? search = null);
    Task<Customer?> GetByIdWithTransactionsAsync(int id);
    Task<Customer?> GetByIdAsync(int id);
    Task<Customer> AddAsync(Customer customer);
    Task<Customer> UpdateAsync(Customer customer);
    Task<Customer> CreateCustomerWithOpeningBalanceAsync(Customer customer, decimal openingAmount, string openingType);
    Task DeleteAsync(Customer customer);
    Task<bool> ExistsAsync(int id);
    Task<bool> PhoneExistsAsync(string phone, int? excludeId = null);
    Task<RemindPe.Models.DTOs.DashboardStatsDto> GetDashboardStatsAsync();
}
