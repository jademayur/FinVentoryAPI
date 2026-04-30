using FinVentoryAPI.DTOs.OpeningItemBalanceDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IOpeningItemBalanceService
    {
        Task<OpeningItemBalanceResponseDto> SaveAsync(OpeningBalanceItemDto dto);
        Task<List<OpeningBalanceMatItemResponseDto>> GetAsync();
        Task<bool> DeleteAsync();
    }
}
