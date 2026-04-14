using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.SalesInvoiceDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface ISalesInvoiceService
    {
        Task<SalesInvoiceResponseDto> CreateAsync(CreateSalesInvoiceMainDto dto);

        Task<List<SalesInvoiceResponseDto>> GetAllAsync();

        Task<SalesInvoiceResponseDto?> GetByIdAsync(int id);

        Task<bool> UpdateAsync(int id, UpdateSalesInvoiceMainDto dto);

        Task<bool> DeleteAsync(int id);

       


        Task<PagedResponseDto<SalesInvoiceResponseDto>> GetPagedAsync(PagedRequestDto request);
    }
}
