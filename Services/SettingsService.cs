using RemindPe.Common;
using RemindPe.Models.DTOs;
using RemindPe.Repositories;

namespace RemindPe.Services;

public class SettingsService : ISettingsService
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly ILogger<SettingsService> _logger;

    public SettingsService(ISettingsRepository settingsRepository, ILogger<SettingsService> logger)
    {
        _settingsRepository = settingsRepository;
        _logger = logger;
    }

    public async Task<MerchantSettingsDto> GetSettingsAsync()
    {
        var settings = await _settingsRepository.GetSettingsAsync();
        return new MerchantSettingsDto(
            settings.StoreName,
            settings.OwnerName,
            settings.Phone,
            settings.UpiId,
            settings.Currency,
            settings.DefaultReminderTemplate,
            settings.ReceiptFooter
        );
    }

    public async Task<MerchantSettingsDto> UpdateSettingsAsync(UpdateMerchantSettingsDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.StoreName))
        {
            throw new ValidationException("Store name cannot be empty.");
        }

        var settings = await _settingsRepository.GetSettingsAsync();
        settings.StoreName = dto.StoreName.Trim();
        settings.OwnerName = dto.OwnerName?.Trim() ?? string.Empty;
        settings.Phone = dto.Phone?.Trim() ?? string.Empty;
        settings.UpiId = string.IsNullOrWhiteSpace(dto.UpiId) ? "merchant@upi" : dto.UpiId.Trim();
        settings.Currency = string.IsNullOrWhiteSpace(dto.Currency) ? "₹" : dto.Currency.Trim();
        settings.DefaultReminderTemplate = string.IsNullOrWhiteSpace(dto.DefaultReminderTemplate) ? "hinglish" : dto.DefaultReminderTemplate.Trim().ToLower();
        settings.ReceiptFooter = dto.ReceiptFooter?.Trim();

        var updated = await _settingsRepository.UpdateSettingsAsync(settings);
        _logger.LogInformation("Merchant settings updated for store: {StoreName}", updated.StoreName);

        return new MerchantSettingsDto(
            updated.StoreName,
            updated.OwnerName,
            updated.Phone,
            updated.UpiId,
            updated.Currency,
            updated.DefaultReminderTemplate,
            updated.ReceiptFooter
        );
    }
}
