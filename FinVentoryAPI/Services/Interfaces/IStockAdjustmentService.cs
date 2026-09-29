using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.StockAdjustmentDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IStockAdjustmentService
    {
        Task<StockAdjustmentResponseDto> CreateAsync(CreateStockAdjustmentMainDto dto);
        Task<bool> UpdateAsync(int id, UpdateStockAdjustmentMainDto dto);
        Task<bool> DeleteAsync(int id);
        Task<StockAdjustmentResponseDto?> GetByIdAsync(int id);
        Task<PagedResponseDto<StockAdjustmentListDto>> GetPagedAsync(PagedRequestDto request);
        Task<StockAdjustmentResponseDto> ConfirmAsync(int id);
        Task<StockAdjustmentResponseDto> CancelAsync(int id);
    }
}
