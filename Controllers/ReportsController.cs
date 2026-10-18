using Microsoft.AspNetCore.Mvc;
using RemindPe.Models.DTOs;
using RemindPe.Services;

namespace RemindPe.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportsService _reportsService;

    public ReportsController(IReportsService reportsService)
    {
        _reportsService = reportsService;
    }

    /// <summary>
    /// GET /api/reports/summary
    /// Generates financial ledger reconciliation summary over a specified date range.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(LedgerReportSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<LedgerReportSummaryDto>> GetSummary(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var summary = await _reportsService.GetSummaryReportAsync(startDate, endDate);
        return Ok(summary);
    }
}
