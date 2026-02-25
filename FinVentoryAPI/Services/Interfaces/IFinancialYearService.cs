using FinVentoryAPI.DTOs.FinancialYearDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IFinancialYearService
    {
        Task<IEnumerable<object>> GetByCompanyAsync(int companyId);
        Task<string> CreateAsync(CreateFinancialYearDto dto);
        Task<string> UpdateAsync(UpdateFinancialYearDto dto);
        Task<string> CloseAsync(int id);
        Task<string> DeleteAsync(int id);
        Task<object?> GetByIdAsync(int id);
    }
}
