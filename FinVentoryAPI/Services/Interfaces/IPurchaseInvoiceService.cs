using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.PurchaseInvoiceDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IPurchaseInvoiceService
    {
        Task<PurchaseInvoiceResponseDto> CreateAsync(CreatePurchaseInvoiceMainDto dto);
        Task<List<PurchaseInvoiceResponseDto>> GetAllAsync();
        Task<PurchaseInvoiceResponseDto?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(int id, UpdatePurchaseInvoiceMainDto dto);
        Task<bool> DeleteAsync(int id);
        Task<PagedResponseDto<PurchaseInvoiceResponseDto>> GetPagedAsync(PagedRequestDto request);
    }
}
