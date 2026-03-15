using FinVentoryAPI.DTOs.AccountDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.TaxTDOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface ITaxService
    {
        Task<TaxResponseDto> CreateAsync(CreateTaxDto dto);
        Task<bool> UpdateAsync(int id,UpdateTaxDto dto);
        Task<bool> DeleteAsync(int id);
        Task<TaxResponseDto> GetByIdAsync(int id);
        Task<List<TaxResponseDto>> GetAllAsync();
        Task<PagedResponseDto<TaxResponseDto>> GetPagedAsync(PagedRequestDto request);
    }
}
