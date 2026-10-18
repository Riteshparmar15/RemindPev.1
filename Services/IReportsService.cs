using RemindPe.Models.DTOs;

namespace RemindPe.Services;

public interface IReportsService
{
    Task<LedgerReportSummaryDto> GetSummaryReportAsync(DateTime? startDate = null, DateTime? endDate = null);
}
