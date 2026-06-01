using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.SalesReturnDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface ISalesReturnService
    {
        Task<SalesReturnResponseDto> CreateAsync(CreateSalesReturnMainDto dto);
        Task<bool> UpdateAsync(int id, UpdateSalesReturnMainDto dto);
        Task<bool> DeleteAsync(int id);
        Task<SalesReturnResponseDto?> GetByIdAsync(int id);
        Task<List<SalesReturnResponseDto>> GetAllAsync();
        Task<PagedResponseDto<SalesReturnResponseDto>> GetPagedAsync(PagedRequestDto request);
    }
}
