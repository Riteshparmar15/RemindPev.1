using Moq;
using RemindPe.Common;
using RemindPe.Models;
using RemindPe.Repositories;
using RemindPe.Services;
using Xunit;

namespace RemindPe.Tests;

public class WhatsAppReminderServiceTests
{
    private readonly Mock<ICustomerRepository> _customerRepoMock;
    private readonly WhatsAppReminderService _reminderService;

    public WhatsAppReminderServiceTests()
    {
        _customerRepoMock = new Mock<ICustomerRepository>();
        _reminderService = new WhatsAppReminderService(_customerRepoMock.Object);
    }

    [Fact]
    public async Task GenerateReminderAsync_CustomerNotFound_ThrowsNotFoundException()
    {
        _customerRepoMock.Setup(r => r.GetByIdWithTransactionsAsync(999))
            .ReturnsAsync((Customer?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _reminderService.GenerateReminderAsync(999));
    }

    [Fact]
    public async Task GenerateReminderAsync_ZeroOrNegativeBalance_ThrowsValidationException()
    {
        var settledCustomer = new Customer
        {
            Id = 1,
            Name = "Suresh",
            Phone = "9876543210",
            Transactions = new List<Transaction>
            {
                new Transaction { Amount = 1000m, Type = "Credit" },
                new Transaction { Amount = 1000m, Type = "Debit" }
            }
        };

        _customerRepoMock.Setup(r => r.GetByIdWithTransactionsAsync(1))
            .ReturnsAsync(settledCustomer);

        var ex = await Assert.ThrowsAsync<ValidationException>(() => _reminderService.GenerateReminderAsync(1));
        Assert.Contains("no outstanding balance", ex.Message);
    }

    [Fact]
    public async Task GenerateReminderAsync_ValidPositiveBalance_GeneratesAccurateWhatsAppUrl()
    {
        var debtor = new Customer
        {
            Id = 2,
            Name = "Mahesh Patel",
            Phone = "09876543210", // 11 digits with leading 0
            Transactions = new List<Transaction>
            {
                new Transaction { Amount = 2500m, Type = "Credit" },
                new Transaction { Amount = 500m, Type = "Debit" }
            }
        };

        _customerRepoMock.Setup(r => r.GetByIdWithTransactionsAsync(2))
            .ReturnsAsync(debtor);

        var reminder = await _reminderService.GenerateReminderAsync(2, "hinglish");

        Assert.Equal(2000m, reminder.PendingBalance);
        Assert.Contains("Namaste Mahesh Patel ji", reminder.FormattedMessage);
        Assert.Contains("₹2,000", reminder.FormattedMessage);
        Assert.StartsWith("https://wa.me/919876543210?text=", reminder.WhatsAppUrl);
    }

    [Fact]
    public async Task GenerateReminderAsync_PreservesPaiseDecimalsWhenPresent()
    {
        var debtor = new Customer
        {
            Id = 3,
            Name = "Kavita",
            Phone = "9811223344",
            Transactions = new List<Transaction>
            {
                new Transaction { Amount = 850.50m, Type = "Credit" }
            }
        };

        _customerRepoMock.Setup(r => r.GetByIdWithTransactionsAsync(3))
            .ReturnsAsync(debtor);

        var reminder = await _reminderService.GenerateReminderAsync(3, "standard");

        Assert.Equal(850.50m, reminder.PendingBalance);
        Assert.Contains("850.50", reminder.FormattedBalance);
        Assert.Contains("850.50", reminder.FormattedMessage);
    }
}
