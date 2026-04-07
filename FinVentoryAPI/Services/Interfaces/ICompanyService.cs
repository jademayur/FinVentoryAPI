using FinVentoryAPI.DTOs.CompanyDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface ICompanyService
    {
        Task<CompanyResponseDto> CreateCompanyAsync(CompanyCreateDto dto, int userId);

        Task<List<CompanyResponseDto>> GetAllCompaniesAsync();

        Task<CompanyResponseDto> GetByIdAsync(int id);

        Task<bool> UpdateCompanyAsync(int id, CompanyUpdateDto dto, int userId);

        Task<bool> DeleteCompanyAsync(int id, int userId);

        Task<CompanyStateDto> GetCompanyStateAsync();
    }
}
