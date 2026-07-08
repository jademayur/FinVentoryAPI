using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.PurchaseReturnDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IPurchaseReturnService
    {
        Task<PurchaseReturnResponseDto> CreateAsync(CreatePurchaseReturnMainDto dto);
        Task<bool> UpdateAsync(int id, UpdatePurchaseReturnMainDto dto);
        Task<bool> DeleteAsync(int id);
        Task<PurchaseReturnResponseDto?> GetByIdAsync(int id);
        Task<List<PurchaseReturnResponseDto>> GetAllAsync();
        Task<PagedResponseDto<PurchaseReturnResponseDto>> GetPagedAsync(PagedRequestDto request);
    }
}
