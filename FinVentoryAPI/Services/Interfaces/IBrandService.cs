using FinVentoryAPI.DTOs.BrandDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IBrandService
    {
        Task<BrandResponseDto> CreateAsync(CreateBrandDto dto);
        Task<List<BrandResponseDto>> GetAllAsync();
        Task<BrandResponseDto?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(int id, UpdateBrandDto dto);
        Task<bool> DeleteAsync(int id);
        Task<PagedResponseDto<BrandResponseDto>> GetPagedAsync(PagedRequestDto request);
    }
}
