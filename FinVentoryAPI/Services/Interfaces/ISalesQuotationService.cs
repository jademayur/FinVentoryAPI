using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.SalesQuotationDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface ISalesQuotationService
    {
        Task<SalesQuotationResponseDto> CreateAsync(CreateSalesQuotationMainDto dto);
        Task<bool> UpdateAsync(int id, UpdateSalesQuotationMainDto dto);
        Task<bool> DeleteAsync(int id);

        Task<SalesQuotationResponseDto?> GetByIdAsync(int id);
        Task<List<SalesQuotationResponseDto>> GetAllAsync();
        Task<PagedResponseDto<SalesQuotationResponseDto>> GetPagedAsync(PagedRequestDto request);
        Task<List<SalesQuotationListDto>> GetByCustomerAsync(int businessPartnerId);
                     
        Task<SalesQuotationResponseDto> CopyAsync(int id, CopySalesQuotationDto? dto = null);

        Task<SalesQuotationResponseDto> ReviseAsync(int id, ReviseSalesQuotationDto dto);
    }
}
