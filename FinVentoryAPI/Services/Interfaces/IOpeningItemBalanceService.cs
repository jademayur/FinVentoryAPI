using FinVentoryAPI.DTOs.OpeningItemBalanceDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IOpeningItemBalanceService
    {
        Task<OpeningItemBalanceResponseDto> SaveAsync(OpeningBalanceItemDto dto);
        Task<List<OpeningBalanceMatItemDto>> GetAsync();
        Task<bool> DeleteAsync();
    }
}
