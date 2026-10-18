using System.ComponentModel.DataAnnotations;

namespace RemindPe.Models.DTOs;

public record CustomerResponseDto(
    int Id,
    string Name,
    string Phone,
    string? Address,
    DateTime CreatedAt,
    decimal TotalCredit,
    decimal TotalDebit,
    decimal NetBalance,
    int TransactionCount,
    DateTime? LastTransactionDate
);

public record CustomerDetailDto(
    int Id,
    string Name,
    string Phone,
    string? Address,
    DateTime CreatedAt,
    decimal TotalCredit,
    decimal TotalDebit,
    decimal NetBalance,
    List<TransactionResponseDto> Transactions
);

public record CreateCustomerDto(
    [Required(ErrorMessage = "Customer name is required.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 120 characters.")]
    string Name,

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^[0-9+\s-]{10,15}$", ErrorMessage = "Please enter a valid 10-digit mobile number.")]
    string Phone,

    [MaxLength(250)]
    string? Address = null,

    [Range(0, 10000000, ErrorMessage = "Opening balance cannot be negative.")]
    decimal? OpeningBalance = null,

    [RegularExpression("^(Credit|Debit)$", ErrorMessage = "Opening type must be 'Credit' or 'Debit'.")]
    string? OpeningType = null
);

public record UpdateCustomerDto(
    [Required(ErrorMessage = "Customer name is required.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 120 characters.")]
    string Name,

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^[0-9+\s-]{10,15}$", ErrorMessage = "Please enter a valid 10-digit mobile number.")]
    string Phone,

    [MaxLength(250)]
    string? Address = null
);

public record TransactionResponseDto(
    int Id,
    int CustomerId,
    decimal Amount,
    string Type,
    string? Note,
    DateTime Date
);

public record TransactionDetailDto(
    int Id,
    int CustomerId,
    string CustomerName,
    string CustomerPhone,
    decimal Amount,
    string Type,
    string? Note,
    DateTime Date
);

public record CreateTransactionDto(
    [Required]
    int CustomerId,

    [Range(0.01, 10000000, ErrorMessage = "Amount must be strictly greater than zero.")]
    decimal Amount,

    [Required]
    [RegularExpression("^(Credit|Debit)$", ErrorMessage = "Type must be either 'Credit' or 'Debit'.")]
    string Type,

    [MaxLength(300)]
    string? Note,

    DateTime? Date
);

public record UpdateTransactionDto(
    [Range(0.01, 10000000, ErrorMessage = "Amount must be strictly greater than zero.")]
    decimal Amount,

    [Required]
    [RegularExpression("^(Credit|Debit)$", ErrorMessage = "Type must be either 'Credit' or 'Debit'.")]
    string Type,

    [MaxLength(300)]
    string? Note,

    DateTime? Date
);

public record TransactionFilterDto(
    int? CustomerId = null,
    string? Type = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int Page = 1,
    int PageSize = 50
);

public record PagedResultDto<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public record DashboardStatsDto(
    decimal TotalReceivable,
    decimal TotalReceived,
    decimal CreditToday,
    decimal PaymentsToday,
    decimal TotalAdvance,
    int TotalCustomers,
    int PendingCustomersCount,
    int SettledCustomersCount,
    int AdvanceCustomersCount,
    int TotalTransactions
);

public record WhatsAppReminderDto(
    int CustomerId,
    string CustomerName,
    string Phone,
    decimal PendingBalance,
    string FormattedBalance,
    string MessageTemplate,
    string FormattedMessage,
    string WhatsAppUrl
);

public record WhatsAppTemplateOptionDto(
    string Key,
    string Title,
    string Description,
    string Preview
);

public record MerchantSettingsDto(
    string StoreName,
    string OwnerName,
    string Phone,
    string UpiId,
    string Currency,
    string DefaultReminderTemplate,
    string? ReceiptFooter
);

public record UpdateMerchantSettingsDto(
    [Required(ErrorMessage = "Store name is required.")]
    [MaxLength(120)]
    string StoreName,

    [MaxLength(120)]
    string? OwnerName,

    [MaxLength(20)]
    string? Phone,

    [MaxLength(100)]
    string? UpiId,

    [MaxLength(10)]
    string? Currency,

    [MaxLength(50)]
    string? DefaultReminderTemplate,

    [MaxLength(200)]
    string? ReceiptFooter
);

public record LedgerReportSummaryDto(
    DateTime StartDate,
    DateTime EndDate,
    decimal TotalCreditGiven,
    decimal TotalPaymentsReceived,
    decimal NetChange,
    int TransactionCount,
    int ActiveCustomersCount,
    decimal TotalOutstandingReceivables,
    int DebtorsCount,
    int SettledCount,
    int AdvanceCount
);
