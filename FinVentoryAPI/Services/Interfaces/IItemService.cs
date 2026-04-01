using FinVentoryAPI.DTOs.AccountDTOs;
using FinVentoryAPI.DTOs.ItemDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using static FinVentoryAPI.DTOs.AccountDTOs.ChartOfAccountDTO;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IItemService
    {
        Task<ItemResponseDto> CreateAsync(CreateItemDto dto);
        Task<bool> UpdateAsync(int id, UpdateItemDto dto);
        Task<bool> DeleteAsync(int id);
        Task<ItemResponseDto> GetByIdAsync(int id);
        Task<List<ItemResponseDto>> GetAllAsync();
        Task<PagedResponseDto<ItemResponseDto>> GetPagedAsync(PagedRequestDto request);
        Task<List<ItemResponseDto>> GetItemListAsync();
        Task<List<SalesInvoiceItemDto>> GetItemsForSalesInvoiceAsync();


    }
}
