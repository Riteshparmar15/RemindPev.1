using RemindPe.Common;
using RemindPe.Models;
using RemindPe.Models.DTOs;
using RemindPe.Repositories;

namespace RemindPe.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(ICustomerRepository customerRepository, ILogger<CustomerService> logger)
    {
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<CustomerResponseDto>> GetAllCustomersAsync(string? search = null)
    {
        var customers = await _customerRepository.GetAllWithTransactionsAsync(search);

        var dtos = customers.Select(c =>
        {
            var credit = c.Transactions.Where(t => t.Type == "Credit").Sum(t => t.Amount);
            var debit = c.Transactions.Where(t => t.Type == "Debit").Sum(t => t.Amount);
            var lastTxDate = c.Transactions.OrderByDescending(t => t.Date).Select(t => (DateTime?)t.Date).FirstOrDefault();

            return new CustomerResponseDto(
                c.Id,
                c.Name,
                c.Phone,
                c.Address,
                c.CreatedAt,
                credit,
                debit,
                credit - debit,
                c.Transactions.Count,
                lastTxDate
            );
        });

        // Sorted with highest pending receivables first
        return dtos.OrderByDescending(c => c.NetBalance).ThenBy(c => c.Name);
    }

    public async Task<CustomerDetailDto> GetCustomerByIdAsync(int id)
    {
        var customer = await _customerRepository.GetByIdWithTransactionsAsync(id);
        if (customer == null)
        {
            throw new NotFoundException($"Customer with ID {id} was not found.");
        }

        var totalCredit = customer.Transactions.Where(t => t.Type == "Credit").Sum(t => t.Amount);
        var totalDebit = customer.Transactions.Where(t => t.Type == "Debit").Sum(t => t.Amount);

        var transactions = customer.Transactions
            .OrderByDescending(t => t.Date)
            .Select(t => new TransactionResponseDto(
                t.Id,
                t.CustomerId,
                t.Amount,
                t.Type,
                t.Note,
                t.Date
            ))
            .ToList();

        return new CustomerDetailDto(
            customer.Id,
            customer.Name,
            customer.Phone,
            customer.Address,
            customer.CreatedAt,
            totalCredit,
            totalDebit,
            totalCredit - totalDebit,
            transactions
        );
    }

    public async Task<CustomerResponseDto> CreateCustomerAsync(CreateCustomerDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ValidationException("Customer name cannot be empty.");
        }

        var normalizedPhone = NormalizeIndianPhoneNumber(dto.Phone);

        // Check for duplicate phone number
        if (await _customerRepository.PhoneExistsAsync(normalizedPhone))
        {
            throw new ValidationException($"A customer account with mobile number {normalizedPhone} already exists.");
        }

        var customer = new Customer
        {
            Name = dto.Name.Trim(),
            Phone = normalizedPhone,
            Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        Customer created;
        decimal credit = 0m;
        decimal debit = 0m;
        int txCount = 0;
        DateTime? lastTx = null;

        // Atomic creation with opening balance if specified
        if (dto.OpeningBalance.HasValue && dto.OpeningBalance.Value > 0)
        {
            var openingType = dto.OpeningType?.Equals("Debit", StringComparison.OrdinalIgnoreCase) == true ? "Debit" : "Credit";
            created = await _customerRepository.CreateCustomerWithOpeningBalanceAsync(customer, dto.OpeningBalance.Value, openingType);
            
            if (openingType == "Credit")
            {
                credit = dto.OpeningBalance.Value;
            }
            else
            {
                debit = dto.OpeningBalance.Value;
            }
            txCount = 1;
            lastTx = DateTime.UtcNow;
            _logger.LogInformation("New customer created with opening balance: {Name} (ID: {Id}, Amount: {Amount}, Type: {Type})",
                created.Name, created.Id, dto.OpeningBalance.Value, openingType);
        }
        else
        {
            created = await _customerRepository.AddAsync(customer);
            _logger.LogInformation("New customer created: {Name} (ID: {Id})", created.Name, created.Id);
        }

        return new CustomerResponseDto(
            created.Id,
            created.Name,
            created.Phone,
            created.Address,
            created.CreatedAt,
            credit,
            debit,
            credit - debit,
            txCount,
            lastTx
        );
    }

    public async Task<CustomerResponseDto> UpdateCustomerAsync(int id, UpdateCustomerDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ValidationException("Customer name cannot be empty.");
        }

        var customer = await _customerRepository.GetByIdWithTransactionsAsync(id);
        if (customer == null)
        {
            throw new NotFoundException($"Customer with ID {id} was not found.");
        }

        var normalizedPhone = NormalizeIndianPhoneNumber(dto.Phone);

        // Check for duplicate phone against other customers
        if (await _customerRepository.PhoneExistsAsync(normalizedPhone, id))
        {
            throw new ValidationException($"Another customer account with mobile number {normalizedPhone} already exists.");
        }

        customer.Name = dto.Name.Trim();
        customer.Phone = normalizedPhone;
        customer.Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim();

        await _customerRepository.UpdateAsync(customer);
        _logger.LogInformation("Customer updated: {Name} (ID: {Id})", customer.Name, customer.Id);

        var credit = customer.Transactions.Where(t => t.Type == "Credit").Sum(t => t.Amount);
        var debit = customer.Transactions.Where(t => t.Type == "Debit").Sum(t => t.Amount);
        var lastTxDate = customer.Transactions.OrderByDescending(t => t.Date).Select(t => (DateTime?)t.Date).FirstOrDefault();

        return new CustomerResponseDto(
            customer.Id,
            customer.Name,
            customer.Phone,
            customer.Address,
            customer.CreatedAt,
            credit,
            debit,
            credit - debit,
            customer.Transactions.Count,
            lastTxDate
        );
    }

    public async Task DeleteCustomerAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
        {
            throw new NotFoundException($"Customer with ID {id} was not found.");
        }

        await _customerRepository.DeleteAsync(customer);
        _logger.LogInformation("Customer deleted: ID {Id}", id);
    }

    public static string NormalizeIndianPhoneNumber(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("91") && digits.Length == 12)
        {
            digits = digits[2..];
        }
        else if (digits.StartsWith("0") && digits.Length == 11)
        {
            digits = digits[1..];
        }

        if (digits.Length != 10)
        {
            throw new ValidationException("Please enter a valid 10-digit mobile number.");
        }

        return digits;
    }
}
