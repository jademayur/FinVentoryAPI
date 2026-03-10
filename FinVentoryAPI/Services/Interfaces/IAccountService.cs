using FinVentoryAPI.DTOs.AccountDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;

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
    }
}
