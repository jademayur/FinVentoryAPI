using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.StockTransferDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IStockTransferService
    {
        Task<StockTransferResponseDto> CreateAsync(CreateStockTransferMainDto dto);
        Task<bool> UpdateAsync(int id, UpdateStockTransferMainDto dto);
        Task<bool> DeleteAsync(int id);
        Task<StockTransferResponseDto?> GetByIdAsync(int id);
        Task<PagedResponseDto<StockTransferListDto>> GetPagedAsync(PagedRequestDto request);
        Task<StockTransferResponseDto> ConfirmAsync(int id);
        Task<StockTransferResponseDto> CancelAsync(int id);
    }
}
