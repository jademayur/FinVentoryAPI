// Services/Interfaces/IStockLedgerService.cs
using FinVentoryAPI.DTOs.StockLedgerDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IStockLedgerService
    {
        // ── Called from voucher post/cancel ───────────
        Task AddEntryAsync(
            int companyId, int itemId, int? warehouseId,
            DateTime date, string voucherType, string voucherNo,
            int? businessPartnerId, decimal qty,
            decimal? rate = null, string? remarks = null,
            int? createdBy = null);

        Task AddEntriesAsync(
            int companyId, int? warehouseId,
            DateTime date, string voucherType, string voucherNo,
            int? businessPartnerId,
            List<StockLedgerLineDto> lines,
            int? createdBy = null);

        Task ReverseEntriesAsync(
            int companyId, string originalVoucherNo,
            string reversalVoucherNo, DateTime reversalDate,
            int? modifiedBy = null);

        // ── Called from API endpoints ──────────────────
        Task<StockLedgerResponseDto?> GetLedgerByItemAsync(
            int itemId, DateTime? from, DateTime? to);

        Task<List<StockLedgerResponseDto>> GetAllLedgersAsync(
            DateTime? from, DateTime? to, int? itemGroupId);

        Task<bool> DeleteEntryAsync(int ledgerId);
    }
}