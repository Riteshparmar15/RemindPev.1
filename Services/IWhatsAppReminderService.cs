using RemindPe.Models.DTOs;

namespace RemindPe.Services;

public interface IWhatsAppReminderService
{
    Task<WhatsAppReminderDto> GenerateReminderAsync(int customerId, string? templateKey = null, string? customNote = null);
    IEnumerable<WhatsAppTemplateOptionDto> GetAvailableTemplates();
}
