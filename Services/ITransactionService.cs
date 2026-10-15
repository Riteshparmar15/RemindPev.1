using RemindPe.Models.DTOs;

namespace RemindPe.Services;

public interface ITransactionService
{
    Task<TransactionResponseDto> GetTransactionByIdAsync(int id);
    Task<IEnumerable<TransactionResponseDto>> GetTransactionsByCustomerIdAsync(int customerId);
    Task<PagedResultDto<TransactionDetailDto>> GetTransactionsAsync(TransactionFilterDto filter);
    Task<TransactionResponseDto> CreateTransactionAsync(CreateTransactionDto dto);
    Task DeleteTransactionAsync(int id);
}
