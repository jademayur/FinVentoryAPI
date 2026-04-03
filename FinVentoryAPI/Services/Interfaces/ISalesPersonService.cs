using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.SalesPersonDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface ISalesPersonService
    {
        Task<List<SalesPersonResponseDto>> GetAllAsync();
        Task<SalesPersonResponseDto?> GetByIdAsync(int id);
        Task<SalesPersonResponseDto> CreateAsync(SalesPersonCreateDto dto);
        Task<bool> UpdateAsync(int id, SalesPersonUpdateDto dto);
        Task<bool> DeleteAsync(int id);
        Task<PagedResponseDto<SalesPersonResponseDto>> GetPagedAsync(PagedRequestDto request);
    }
}
