using FinVentoryAPI.DTOs.UserCompany;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IUserCompanyService
    {
        Task<List<UserCompanyResponseDto>> GetAllAsync();
        Task<UserCompanyResponseDto?> CreateAsync(UserCompanyCreateDto dto);
        Task<bool> UpdateAsync(int id, UserCompanyCreateDto dto);
        Task<bool> DeleteAsync(int id);
        Task<int> BulkCreateAsync(UserCompanyBulkCreateDto dto);
    }
}
