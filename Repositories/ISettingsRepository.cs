using RemindPe.Models;

namespace RemindPe.Repositories;

public interface ISettingsRepository
{
    Task<MerchantSettings> GetSettingsAsync();
    Task<MerchantSettings> UpdateSettingsAsync(MerchantSettings settings);
}
