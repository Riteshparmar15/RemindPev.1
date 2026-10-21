using Microsoft.AspNetCore.Mvc;
using RemindPe.Models.DTOs;
using RemindPe.Services;

namespace RemindPe.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;

    public SettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    /// <summary>
    /// GET /api/settings
    /// Retrieves current merchant store profile and preferences.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(MerchantSettingsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MerchantSettingsDto>> GetSettings()
    {
        var settings = await _settingsService.GetSettingsAsync();
        return Ok(settings);
    }

    /// <summary>
    /// PUT /api/settings
    /// Updates merchant store profile (Store name, owner, UPI ID, currency, reminder templates).
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(MerchantSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MerchantSettingsDto>> UpdateSettings([FromBody] UpdateMerchantSettingsDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _settingsService.UpdateSettingsAsync(dto);
        return Ok(updated);
    }

    /// <summary>
    /// POST /api/settings/seed-demo
    /// Populates full realistic demo dataset (18 customers, 45+ transactions, aging buckets).
    /// </summary>
    [HttpPost("seed-demo")]
    [ProducesResponseType(typeof(MerchantSettingsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MerchantSettingsDto>> SeedDemo()
    {
        var settings = await _settingsService.SeedDemoDataAsync();
        return Ok(settings);
    }
}
