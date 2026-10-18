using RemindPe.Models.DTOs;

namespace RemindPe.Services;

public interface ISettingsService
{
    Task<MerchantSettingsDto> GetSettingsAsync();
    Task<MerchantSettingsDto> UpdateSettingsAsync(UpdateMerchantSettingsDto dto);
}
