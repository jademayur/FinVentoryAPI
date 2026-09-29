using FinVentoryAPI.DTOs.GRNDTOs;
using FinVentoryAPI.DTOs.Gstr1DTOs;
using FinVentoryAPI.DTOs.Gstr3bDTOs;
using FinVentoryAPI.DTOs.Gstr9DTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IGSTReportsService
    {
        // ── GSTR-3B ──────────────────────────────────────────────────────────
        Task<Gstr3bResponseDto> GetSummaryAsync(string? dateFrom, string? dateTo);
        Task<List<Gstr3bInvoiceListDto>> GetSalesInvoicesAsync(string? dateFrom, string? dateTo);
        Task<List<Gstr3bInvoiceListDto>> GetPurchaseInvoicesAsync(string? dateFrom, string? dateTo);

        // ── GSTR-1 ───────────────────────────────────────────────────────────
        Task<Gstr1ResponseDto> GetGstr1SummaryAsync(string? dateFrom, string? dateTo);
        Task<List<Gstr1B2BRowDto>> GetGstr1B2BAsync(string? dateFrom, string? dateTo);

        // Table 9B
        Task<Gstr1CdnrSummaryDto> GetGstr1CdnrAsync(string? dateFrom, string? dateTo);
        Task<Gstr1CdnurSummaryDto> GetGstr1CdnurAsync(string? dateFrom, string? dateTo);

        // Table 12
        Task<Gstr1HsnSummaryDto> GetGstr1HsnSummaryAsync(string? dateFrom, string? dateTo);

        // Table 13
        Task<Gstr1DocSeriesSummaryDto> GetGstr1DocSeriesAsync(string? dateFrom, string? dateTo);

        // ── GSTR-9 ───────────────────────────────────────────────────────────
        Task<Gstr9ResponseDto> GetGstr9SummaryAsync(int year);
        Task<List<Gstr9MonthlyBreakdownDto>> GetGstr9MonthlyBreakdownAsync(int year);
    }
}
