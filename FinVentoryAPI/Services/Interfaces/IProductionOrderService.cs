using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.ProductionDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IProductionOrderService
    {
        Task<ProductionOrderResponseDto> CreateAsync(CreateProductionOrderDto dto);
        Task<bool> UpdateAsync(int id, UpdateProductionOrderDto dto);
        Task<bool> DeleteAsync(int id);
        Task<ProductionOrderResponseDto?> GetByIdAsync(int id);
        Task<PagedResponseDto<ProductionOrderListItemDto>> GetPagedAsync(PagedRequestDto request);
        Task<bool> CompleteAsync(int id, CompleteProductionOrderDto dto);
        Task<bool> CancelAsync(int id);
        Task<bool> SetInProgressAsync(int id);
    }
}
