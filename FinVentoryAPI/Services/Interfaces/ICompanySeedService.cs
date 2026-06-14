using FinVentoryAPI.DTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface ICompanySeedService
    {
        Task<SeedResultDto> SeedAllAsync(int companyId, int userId);
    }
}
