using FinVentoryAPI.DTOs.AccountDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using static FinVentoryAPI.DTOs.AccountDTOs.ChartOfAccountDTO;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IAccountService
    {
        Task<AccountResponseDto> CreateAsync(CreateAccountDto dto);
        Task<bool> UpdateAsync(int id, UpdateAccountDto dto);
        Task<bool> DeleteAsync(int id);
        Task<AccountResponseDto> GetByIdAsync(int id);
        Task<List<AccountResponseDto>> GetAllAsync();
        Task<PagedResponseDto<AccountResponseDto>> GetPagedAsync(PagedRequestDto request);
        Task<List<ChartOfAccountNodeDto>> GetChartOfAccountsAsync();
        Task<List<AccountResponseDto>> GetBalanceSheetAccountsAsync();
    }
}
