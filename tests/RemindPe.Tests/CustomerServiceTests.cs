using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RemindPe.Common;
using RemindPe.Models;
using RemindPe.Models.DTOs;
using RemindPe.Repositories;
using RemindPe.Services;
using Xunit;

namespace RemindPe.Tests;

public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _customerRepoMock;
    private readonly CustomerService _customerService;

    public CustomerServiceTests()
    {
        _customerRepoMock = new Mock<ICustomerRepository>();
        _customerService = new CustomerService(_customerRepoMock.Object, NullLogger<CustomerService>.Instance);
    }

    [Theory]
    [InlineData("9876543210", "9876543210")]
    [InlineData("+91 9876543210", "9876543210")]
    [InlineData("09876543210", "9876543210")]
    [InlineData("91-9876543210", "9876543210")]
    [InlineData("  9876543210  ", "9876543210")]
    public void NormalizeIndianPhoneNumber_ValidFormats_NormalizesTo10Digits(string input, string expected)
    {
        var result = CustomerService.NormalizeIndianPhoneNumber(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("abcdefghij")]
    [InlineData("12345678901234567")]
    [InlineData("")]
    public void NormalizeIndianPhoneNumber_InvalidFormats_ThrowsValidationException(string input)
    {
        Assert.Throws<ValidationException>(() => CustomerService.NormalizeIndianPhoneNumber(input));
    }

    [Fact]
    public async Task CreateCustomerAsync_DuplicatePhone_ThrowsValidationException()
    {
        // Arrange
        _customerRepoMock.Setup(r => r.PhoneExistsAsync("9876543210", null))
            .ReturnsAsync(true);

        var dto = new CreateCustomerDto("Ramesh Kumar", "9876543210");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => _customerService.CreateCustomerAsync(dto));
        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public async Task CreateCustomerAsync_WithCreditOpeningBalance_CreatesAtomically()
    {
        // Arrange
        _customerRepoMock.Setup(r => r.PhoneExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);

        _customerRepoMock.Setup(r => r.CreateCustomerWithOpeningBalanceAsync(It.IsAny<Customer>(), 1500m, "Credit"))
            .ReturnsAsync((Customer c, decimal amt, string type) =>
            {
                c.Id = 42;
                return c;
            });

        var dto = new CreateCustomerDto("Sunita Sharma", "9876543210", null, 1500m, "Credit");

        // Act
        var result = await _customerService.CreateCustomerAsync(dto);

        // Assert
        Assert.Equal(42, result.Id);
        Assert.Equal("Sunita Sharma", result.Name);
        Assert.Equal(1500m, result.TotalCredit);
        Assert.Equal(0m, result.TotalDebit);
        Assert.Equal(1500m, result.NetBalance);
        Assert.Equal(1, result.TransactionCount);
        _customerRepoMock.Verify(r => r.CreateCustomerWithOpeningBalanceAsync(It.IsAny<Customer>(), 1500m, "Credit"), Times.Once);
    }

    [Fact]
    public async Task GetAllCustomersAsync_AccuratelyCalculatesNetBalances()
    {
        // Arrange: Ramesh has 2000 credit, 500 debit -> Net 1500 (Due)
        //          Priya has 1000 credit, 1500 debit -> Net -500 (Advance)
        //          Rahul has 800 credit, 800 debit   -> Net 0 (Settled)
        var customers = new List<Customer>
        {
            new Customer
            {
                Id = 1,
                Name = "Ramesh",
                Phone = "9876543210",
                Transactions = new List<Transaction>
                {
                    new Transaction { Amount = 2000m, Type = "Credit", Date = DateTime.UtcNow },
                    new Transaction { Amount = 500m, Type = "Debit", Date = DateTime.UtcNow }
                }
            },
            new Customer
            {
                Id = 2,
                Name = "Priya",
                Phone = "9123456789",
                Transactions = new List<Transaction>
                {
                    new Transaction { Amount = 1000m, Type = "Credit", Date = DateTime.UtcNow },
                    new Transaction { Amount = 1500m, Type = "Debit", Date = DateTime.UtcNow }
                }
            },
            new Customer
            {
                Id = 3,
                Name = "Rahul",
                Phone = "9988776655",
                Transactions = new List<Transaction>
                {
                    new Transaction { Amount = 800m, Type = "Credit", Date = DateTime.UtcNow },
                    new Transaction { Amount = 800m, Type = "Debit", Date = DateTime.UtcNow }
                }
            }
        };

        _customerRepoMock.Setup(r => r.GetAllWithTransactionsAsync(null))
            .ReturnsAsync(customers);

        // Act
        var result = (await _customerService.GetAllCustomersAsync()).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        // Sorted highest pending balance first
        Assert.Equal(1500m, result[0].NetBalance); // Ramesh
        Assert.Equal("Ramesh", result[0].Name);

        Assert.Equal(0m, result[1].NetBalance); // Rahul
        Assert.Equal("Rahul", result[1].Name);

        Assert.Equal(-500m, result[2].NetBalance); // Priya (Advance)
        Assert.Equal("Priya", result[2].Name);
    }
}
