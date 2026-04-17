using FinVentoryAPI.DTOs.AccountLedgerPostingDTOs;
using FinVentoryAPI.DTOs.ItemDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IAccountLedgerService
    {
        Task<AccountLedgerResponseDto?> GetLedgerByAccountAsync(
                int accountId, DateTime? from, DateTime? to);

        Task<List<AccountLedgerResponseDto>> GetAllLedgersAsync(
            DateTime? from, DateTime? to, int? accountGroupId);
    }
}
