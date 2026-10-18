using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RemindPe.Common;
using RemindPe.Models;
using RemindPe.Models.DTOs;
using RemindPe.Repositories;
using RemindPe.Services;
using Xunit;

namespace RemindPe.Tests;

public class SettingsServiceTests
{
    private readonly Mock<ISettingsRepository> _settingsRepoMock;
    private readonly SettingsService _settingsService;

    public SettingsServiceTests()
    {
        _settingsRepoMock = new Mock<ISettingsRepository>();
        _settingsService = new SettingsService(_settingsRepoMock.Object, NullLogger<SettingsService>.Instance);
    }

    [Fact]
    public async Task GetSettingsAsync_ReturnsConfiguredSettings()
    {
        var settings = new MerchantSettings
        {
            StoreName = "Radhe Kirana Store",
            OwnerName = "Radheshyam",
            Phone = "9876543210",
            UpiId = "radhe@okicici",
            Currency = "₹",
            DefaultReminderTemplate = "hinglish"
        };

        _settingsRepoMock.Setup(r => r.GetSettingsAsync())
            .ReturnsAsync(settings);

        var result = await _settingsService.GetSettingsAsync();

        Assert.Equal("Radhe Kirana Store", result.StoreName);
        Assert.Equal("Radheshyam", result.OwnerName);
        Assert.Equal("radhe@okicici", result.UpiId);
        Assert.Equal("hinglish", result.DefaultReminderTemplate);
    }

    [Fact]
    public async Task UpdateSettingsAsync_EmptyStoreName_ThrowsValidationException()
    {
        var dto = new UpdateMerchantSettingsDto(
            StoreName: "",
            OwnerName: "Owner",
            Phone: "9876543210",
            UpiId: "test@upi",
            Currency: "₹",
            DefaultReminderTemplate: "hinglish",
            ReceiptFooter: "Thank you"
        );

        await Assert.ThrowsAsync<ValidationException>(() => _settingsService.UpdateSettingsAsync(dto));
    }

    [Fact]
    public async Task UpdateSettingsAsync_ValidDto_UpdatesAndPersists()
    {
        var existing = new MerchantSettings { StoreName = "Old Name" };
        _settingsRepoMock.Setup(r => r.GetSettingsAsync())
            .ReturnsAsync(existing);
        _settingsRepoMock.Setup(r => r.UpdateSettingsAsync(It.IsAny<MerchantSettings>()))
            .ReturnsAsync((MerchantSettings s) => s);

        var dto = new UpdateMerchantSettingsDto(
            StoreName: "New Vyapar Hub",
            OwnerName: "Ritesh",
            Phone: "9876543210",
            UpiId: "ritesh@upi",
            Currency: "₹",
            DefaultReminderTemplate: "formal",
            ReceiptFooter: "Visit again"
        );

        var result = await _settingsService.UpdateSettingsAsync(dto);

        Assert.Equal("New Vyapar Hub", result.StoreName);
        Assert.Equal("ritesh@upi", result.UpiId);
        Assert.Equal("formal", result.DefaultReminderTemplate);
    }
}
