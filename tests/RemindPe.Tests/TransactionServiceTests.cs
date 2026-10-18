using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RemindPe.Common;
using RemindPe.Models;
using RemindPe.Models.DTOs;
using RemindPe.Repositories;
using RemindPe.Services;
using Xunit;

namespace RemindPe.Tests;

public class TransactionServiceTests
{
    private readonly Mock<ITransactionRepository> _txRepoMock;
    private readonly Mock<ICustomerRepository> _customerRepoMock;
    private readonly TransactionService _txService;

    public TransactionServiceTests()
    {
        _txRepoMock = new Mock<ITransactionRepository>();
        _customerRepoMock = new Mock<ICustomerRepository>();
        _txService = new TransactionService(
            _txRepoMock.Object,
            _customerRepoMock.Object,
            NullLogger<TransactionService>.Instance
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public async Task CreateTransactionAsync_ZeroOrNegativeAmount_ThrowsValidationException(decimal amount)
    {
        var dto = new CreateTransactionDto(1, amount, "Credit", "Test note", DateTime.UtcNow);
        await Assert.ThrowsAsync<ValidationException>(() => _txService.CreateTransactionAsync(dto));
    }

    [Fact]
    public async Task CreateTransactionAsync_CustomerDoesNotExist_ThrowsNotFoundException()
    {
        _customerRepoMock.Setup(r => r.ExistsAsync(999))
            .ReturnsAsync(false);

        var dto = new CreateTransactionDto(999, 100m, "Credit", "Test note", DateTime.UtcNow);
        await Assert.ThrowsAsync<NotFoundException>(() => _txService.CreateTransactionAsync(dto));
    }

    [Fact]
    public async Task CreateTransactionAsync_ValidInput_CreatesSuccessfully()
    {
        _customerRepoMock.Setup(r => r.ExistsAsync(1))
            .ReturnsAsync(true);

        _txRepoMock.Setup(r => r.AddAsync(It.IsAny<Transaction>()))
            .ReturnsAsync((Transaction t) =>
            {
                t.Id = 101;
                return t;
            });

        var dto = new CreateTransactionDto(1, 500m, "Credit", "Rice and oil", DateTime.UtcNow);
        var result = await _txService.CreateTransactionAsync(dto);

        Assert.Equal(101, result.Id);
        Assert.Equal(500m, result.Amount);
        Assert.Equal("Credit", result.Type);
        Assert.Equal("Rice and oil", result.Note);
    }

    [Fact]
    public async Task UpdateTransactionAsync_ValidUpdate_UpdatesAndReturnsDto()
    {
        var existingTx = new Transaction
        {
            Id = 5,
            CustomerId = 1,
            Amount = 500m,
            Type = "Credit",
            Note = "Old note"
        };

        _txRepoMock.Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(existingTx);

        _txRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Transaction>()))
            .ReturnsAsync((Transaction t) => t);

        var updateDto = new UpdateTransactionDto(750m, "Debit", "Updated note", DateTime.UtcNow);
        var result = await _txService.UpdateTransactionAsync(5, updateDto);

        Assert.Equal(5, result.Id);
        Assert.Equal(750m, result.Amount);
        Assert.Equal("Debit", result.Type);
        Assert.Equal("Updated note", result.Note);
    }
}
