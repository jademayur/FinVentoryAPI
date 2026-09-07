using FinVentoryAPI.DTOs.CompanyConfigDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface ICompanyConfigService
    {
        Task<List<CompanyConfigDto>> GetAllAsync(int companyId);
        Task<CompanyConfigDto?> GetByKeyAsync(int companyId, string key);
        Task<string?> GetValueAsync(int companyId, string key);
        Task UpsertAsync(int companyId, UpsertCompanyConfigDto dto);
        Task BulkUpsertAsync(int companyId, BulkUpsertCompanyConfigDto dto);
        Task DeleteAsync(int companyId, int configId);
    }
}
