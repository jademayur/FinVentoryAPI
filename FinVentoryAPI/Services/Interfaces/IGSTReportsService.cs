using FinVentoryAPI.DTOs.Gstr1DTOs;
using FinVentoryAPI.DTOs.Gstr3bDTOs;
using FinVentoryAPI.DTOs.Gstr9DTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IGSTReportsService
    {
        // ── GSTR-3 ───────────────────────────────────────────────────────────
        Task<Gstr3bResponseDto> GetSummaryAsync(string taxPeriod);
        Task<List<Gstr3bInvoiceListDto>> GetSalesInvoicesAsync(string taxPeriod);
        Task<List<Gstr3bInvoiceListDto>> GetPurchaseInvoicesAsync(string taxPeriod);

        // ── GSTR-1 ───────────────────────────────────────────────────────────
        Task<Gstr1ResponseDto> GetGstr1SummaryAsync(string taxPeriod);
        Task<List<Gstr1B2BRowDto>> GetGstr1B2BAsync(string taxPeriod);

        // ── GSTR-9 ───────────────────────────────────────────────────────────
        Task<Gstr9ResponseDto> GetGstr9SummaryAsync(int year);
        Task<List<Gstr9MonthlyBreakdownDto>> GetGstr9MonthlyBreakdownAsync(int year);
    }
}
