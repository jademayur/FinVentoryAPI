using FinVentoryAPI.DTOs.UserCompany;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IUserCompanyService
    {
        Task<IEnumerable<object>> GetAllAsync();
        Task<object> GetByUserAsync(int userId);
        Task<string> CreateAsync(UserCompanyCreateDto dto);
        Task<string> UpdateAsync(UserCompanyUpdateDto dto);
        Task<string> DeleteAsync(int id);
    }
}
