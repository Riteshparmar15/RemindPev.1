using Microsoft.EntityFrameworkCore;
using RemindPe.Data;
using RemindPe.Models;

namespace RemindPe.Repositories;

public class SettingsRepository : ISettingsRepository
{
    private readonly KhataDbContext _context;

    public SettingsRepository(KhataDbContext context)
    {
        _context = context;
    }

    public async Task<MerchantSettings> GetSettingsAsync()
    {
        var settings = await _context.Settings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new MerchantSettings
            {
                StoreName = "Apex Trading & Services",
                OwnerName = "Ritesh Parmar",
                Phone = "9876543210",
                UpiId = "merchant@upi",
                Currency = "₹",
                DefaultReminderTemplate = "hinglish",
                ReceiptFooter = "Thank you for your business! Please visit again.",
                UpdatedAt = DateTime.UtcNow
            };
            _context.Settings.Add(settings);
            await _context.SaveChangesAsync();
        }
        return settings;
    }

    public async Task<MerchantSettings> UpdateSettingsAsync(MerchantSettings settings)
    {
        settings.UpdatedAt = DateTime.UtcNow;
        _context.Settings.Update(settings);
        await _context.SaveChangesAsync();
        return settings;
    }
}
