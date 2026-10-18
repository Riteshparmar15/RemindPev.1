using RemindPe.Common;
using RemindPe.Models;
using RemindPe.Models.DTOs;
using RemindPe.Repositories;

namespace RemindPe.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        ITransactionRepository transactionRepository,
        ICustomerRepository customerRepository,
        ILogger<TransactionService> logger)
    {
        _transactionRepository = transactionRepository;
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task<TransactionResponseDto> GetTransactionByIdAsync(int id)
    {
        var tx = await _transactionRepository.GetByIdAsync(id);
        if (tx == null)
        {
            throw new NotFoundException($"Transaction with ID {id} was not found.");
        }

        return new TransactionResponseDto(
            tx.Id,
            tx.CustomerId,
            tx.Amount,
            tx.Type,
            tx.Note,
            tx.Date
        );
    }

    public async Task<IEnumerable<TransactionResponseDto>> GetTransactionsByCustomerIdAsync(int customerId)
    {
        var exists = await _customerRepository.ExistsAsync(customerId);
        if (!exists)
        {
            throw new NotFoundException($"Customer with ID {customerId} was not found.");
        }

        var list = await _transactionRepository.GetByCustomerIdAsync(customerId);
        return list.Select(tx => new TransactionResponseDto(
            tx.Id,
            tx.CustomerId,
            tx.Amount,
            tx.Type,
            tx.Note,
            tx.Date
        ));
    }

    public async Task<PagedResultDto<TransactionDetailDto>> GetTransactionsAsync(TransactionFilterDto filter)
    {
        var (items, totalCount) = await _transactionRepository.GetTransactionsAsync(
            filter.CustomerId,
            filter.Type,
            filter.StartDate,
            filter.EndDate,
            filter.Page,
            filter.PageSize
        );

        var dtos = items.Select(tx => new TransactionDetailDto(
            tx.Id,
            tx.CustomerId,
            tx.Customer?.Name ?? "Unknown",
            tx.Customer?.Phone ?? "",
            tx.Amount,
            tx.Type,
            tx.Note,
            tx.Date
        )).ToList();

        var safePageSize = Math.Clamp(filter.PageSize, 1, 500);
        var totalPages = (int)Math.Ceiling((double)totalCount / safePageSize);

        return new PagedResultDto<TransactionDetailDto>(
            dtos,
            totalCount,
            filter.Page,
            safePageSize,
            totalPages
        );
    }

    public async Task<TransactionResponseDto> CreateTransactionAsync(CreateTransactionDto dto)
    {
        if (dto.Amount <= 0)
        {
            throw new ValidationException("Transaction amount must be strictly greater than zero.");
        }

        var customerExists = await _customerRepository.ExistsAsync(dto.CustomerId);
        if (!customerExists)
        {
            throw new NotFoundException($"Customer with ID {dto.CustomerId} does not exist.");
        }

        var normalizedType = dto.Type.Equals("Debit", StringComparison.OrdinalIgnoreCase) ? "Debit" : "Credit";

        var transaction = new Transaction
        {
            CustomerId = dto.CustomerId,
            Amount = Math.Abs(dto.Amount),
            Type = normalizedType,
            Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim(),
            Date = dto.Date ?? DateTime.UtcNow
        };

        var created = await _transactionRepository.AddAsync(transaction);
        _logger.LogInformation("Recorded {Type} transaction: ₹{Amount} for customer ID {CustomerId}",
            created.Type, created.Amount, created.CustomerId);

        return new TransactionResponseDto(
            created.Id,
            created.CustomerId,
            created.Amount,
            created.Type,
            created.Note,
            created.Date
        );
    }

    public async Task<TransactionResponseDto> UpdateTransactionAsync(int id, UpdateTransactionDto dto)
    {
        if (dto.Amount <= 0)
        {
            throw new ValidationException("Transaction amount must be strictly greater than zero.");
        }

        var tx = await _transactionRepository.GetByIdAsync(id);
        if (tx == null)
        {
            throw new NotFoundException($"Transaction with ID {id} was not found.");
        }

        var normalizedType = dto.Type.Equals("Debit", StringComparison.OrdinalIgnoreCase) ? "Debit" : "Credit";

        tx.Amount = Math.Abs(dto.Amount);
        tx.Type = normalizedType;
        tx.Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim();
        if (dto.Date.HasValue)
        {
            tx.Date = dto.Date.Value;
        }

        var updated = await _transactionRepository.UpdateAsync(tx);
        _logger.LogInformation("Updated transaction ID {Id}: ₹{Amount} ({Type})", updated.Id, updated.Amount, updated.Type);

        return new TransactionResponseDto(
            updated.Id,
            updated.CustomerId,
            updated.Amount,
            updated.Type,
            updated.Note,
            updated.Date
        );
    }

    public async Task DeleteTransactionAsync(int id)
    {
        var tx = await _transactionRepository.GetByIdAsync(id);
        if (tx == null)
        {
            throw new NotFoundException($"Transaction with ID {id} was not found.");
        }

        await _transactionRepository.DeleteAsync(tx);
        _logger.LogInformation("Deleted transaction ID {Id}", id);
    }
}
