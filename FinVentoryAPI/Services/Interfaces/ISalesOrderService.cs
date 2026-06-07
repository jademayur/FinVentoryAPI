using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.SalesOrderDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface ISalesOrderService
    {
        // CRUD
        Task<SalesOrderResponseDto> CreateAsync(CreateSalesOrderMainDto dto);
        Task<bool> UpdateAsync(int id, UpdateSalesOrderMainDto dto);
        Task<bool> DeleteAsync(int id);

        // Queries
        Task<SalesOrderResponseDto?> GetByIdAsync(int id);
        Task<List<SalesOrderResponseDto>> GetAllAsync();
        Task<PagedResponseDto<SalesOrderResponseDto>> GetPagedAsync(PagedRequestDto request);

        // Quotation integration
        Task<List<QuotationPickerDto>> GetQuotationsForCustomerAsync(int businessPartnerId);
        Task<QuotationPrefillDto> GetQuotationPrefillAsync(int quotationId);

        // Status
        Task<bool> ConfirmAsync(int id);
        Task<bool> CancelAsync(int id);
    }
}
