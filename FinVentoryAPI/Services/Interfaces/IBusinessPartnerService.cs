using FinVentoryAPI.DTOs.BusinessPartnerDTOs;
using FinVentoryAPI.DTOs.ItemDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IBusinessPartnerService
    {
        Task<BusinessPartnerResponseDto> CreateAsync(CreateBusinessPartnerDto dto);
        Task<bool> UpdateAsync(int id, UpdateBusinessPartnerDto dto);
        Task<bool> DeleteAsync(int id);
        Task<BusinessPartnerResponseDto> GetByIdAsync(int id);
        Task<List<BusinessPartnerResponseDto>> GetAllAsync();
        Task<PagedResponseDto<BusinessPartnerResponseDto>> GetPagedAsync(PagedRequestDto request);
    }
}
