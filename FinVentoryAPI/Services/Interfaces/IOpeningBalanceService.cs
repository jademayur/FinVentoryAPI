using FinVentoryAPI.DTOs.OpeningBalanceDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IOpeningBalanceService
    {
        Task<OpeningBalanceResponseDto> SaveAsync(OpeningBalanceDto dto);
        Task<List<OpeningBalanceItemDto>> GetAsync();
        Task<bool> DeleteAsync();
    }
}
