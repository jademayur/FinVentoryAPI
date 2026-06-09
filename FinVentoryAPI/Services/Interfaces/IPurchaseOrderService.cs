using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.PurchaseOrderDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IPurchaseOrderService
    {
        // CRUD
        Task<PurchaseOrderResponseDto> CreateAsync(CreatePurchaseOrderMainDto dto);
        Task<bool> UpdateAsync(int id, UpdatePurchaseOrderMainDto dto);
        Task<bool> DeleteAsync(int id);

        // Status transitions
        Task<bool> ConfirmAsync(int id);
        Task<bool> CancelAsync(int id);

        // Queries
        Task<List<PurchaseOrderResponseDto>> GetAllAsync();
        Task<PurchaseOrderResponseDto?> GetByIdAsync(int id);
        Task<PagedResponseDto<PurchaseOrderResponseDto>> GetPagedAsync(PagedRequestDto request);
    }
}
