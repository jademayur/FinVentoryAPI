using FinVentoryAPI.DTOs.HsnDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.TaxTDOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IHsnService
    {
        Task<HsnResponseDto> CreateAsync(CreateHsnDto dto);
        Task<bool> UpdateAsync(int id, UpdateHsnDto dto);
        Task<bool> DeleteAsync(int id);
        Task<HsnResponseDto> GetByIdAsync(int id);
        Task<List<HsnResponseDto>> GetAllAsync();
        Task<PagedResponseDto<HsnResponseDto>> GetPagedAsync(PagedRequestDto request);

    }
}
