using FinVentoryAPI.DTOs.AccountLedgerPostingDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IAccountLedgerPostingService
    {
        Task AddEntryAsync(
        int companyId, int financialYearId,
        int accountId, int? businessPartnerId,
        DateTime date, string voucherType, string voucherNo,
        decimal debit, decimal credit,
        string? remarks = null, int? createdBy = null);

        Task AddEntriesAsync(
            int companyId, int financialYearId,
            DateTime date, string voucherType, string voucherNo,
            List<AccountLedgerLineDto> lines,
            int? createdBy = null);

        Task ReverseEntriesAsync(
            int companyId, int financialYearId,
            string originalVoucherNo,
            string reversalVoucherNo, DateTime reversalDate,
            int? modifiedBy = null);

        Task<AccountLedgerResponseDto?> GetLedgerByAccountAsync(
            int accountId, DateTime? from, DateTime? to);   // ← no change, uses _common

        Task<List<AccountLedgerResponseDto>> GetAllLedgersAsync(
            DateTime? from, DateTime? to, int? accountGroupId);

        Task<bool> DeleteEntryAsync(int postingId);

        Task UpdateEntriesAsync(
    int companyId, int financialYearId,
    DateTime date, string voucherType, string voucherNo,
    List<AccountLedgerLineDto> lines,
    int? modifiedBy = null);

        Task SoftDeleteByVoucherAsync(
            int companyId, int financialYearId,
            string voucherNo, int? modifiedBy = null);
    }
}
