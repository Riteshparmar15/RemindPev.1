using Microsoft.AspNetCore.Mvc;
using RemindPe.Models.DTOs;
using RemindPe.Services;

namespace RemindPe.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RemindersController : ControllerBase
{
    private readonly IWhatsAppReminderService _reminderService;

    public RemindersController(IWhatsAppReminderService reminderService)
    {
        _reminderService = reminderService;
    }

    /// <summary>
    /// GET /api/reminders/whatsapp/{customerId}
    /// Generates a pre-formatted WhatsApp payment reminder with live balance and wa.me link.
    /// </summary>
    [HttpGet("whatsapp/{customerId:int}")]
    [ProducesResponseType(typeof(WhatsAppReminderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WhatsAppReminderDto>> GetWhatsAppReminder(
        int customerId,
        [FromQuery] string? templateKey,
        [FromQuery] string? customNote)
    {
        var reminder = await _reminderService.GenerateReminderAsync(customerId, templateKey, customNote);
        return Ok(reminder);
    }

    /// <summary>
    /// GET /api/reminders/whatsapp/templates
    /// Retrieves available WhatsApp reminder templates (e.g. Standard, Polite Hinglish, Formal).
    /// </summary>
    [HttpGet("whatsapp/templates")]
    [ProducesResponseType(typeof(IEnumerable<WhatsAppTemplateOptionDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<WhatsAppTemplateOptionDto>> GetTemplates()
    {
        var templates = _reminderService.GetAvailableTemplates();
        return Ok(templates);
    }
}
