using FinVentoryAPI.DTOs.CashBankEntryDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.Enums;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface ICashBankEntryService
    {
        Task<CashBankEntryResponseDto> CreateAsync(CreateCashBankEntryDto dto);
        Task<CashBankEntryResponseDto?> GetByIdAsync(int id);
        Task<List<CashBankEntryResponseDto>> GetAllAsync();
        Task<PagedResponseDto<CashBankEntryResponseDto>> GetPagedAsync(PagedRequestDto request);
        Task<bool> UpdateAsync(int id, UpdateCashBankEntryDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
