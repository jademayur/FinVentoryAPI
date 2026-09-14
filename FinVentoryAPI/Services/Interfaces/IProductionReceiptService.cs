using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.ProductionReceiptDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IProductionReceiptService
    {
        Task<ProductionReceiptResponseDto> CreateAsync(CreateProductionReceiptMainDto dto);
        Task<ProductionReceiptResponseDto?> UpdateAsync(int id, UpdateProductionReceiptMainDto dto);
        Task<ProductionReceiptResponseDto?> GetByIdAsync(int id);
        Task<PagedResponseDto<ProductionReceiptListDto>> GetPagedAsync(PagedRequestDto request);
        Task<ProductionReceiptResponseDto> ConfirmAsync(int id);
        Task<ProductionReceiptResponseDto> CancelAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}
