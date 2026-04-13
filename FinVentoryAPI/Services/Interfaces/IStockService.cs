using FinVentoryAPI.DTOs.ItemDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IStockService
    {
        Task<PagedResponseDto<StockDto>> GetPagedAsync(PagedRequestDto request);
        Task<List<object>> GetGroupsAsync();
    }
}
