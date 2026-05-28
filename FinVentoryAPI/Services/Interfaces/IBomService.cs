using FinVentoryAPI.DTOs.BomDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IBomService
    {        
        Task<BomResponseDto> CreateAsync(CreateBomDto dto);
        Task<bool> UpdateAsync(int id, UpdateBomDto dto);
        Task<bool> DeleteAsync(int id);       
        Task<BomResponseDto?> GetByIdAsync(int id);        
        Task<PagedResponseDto<BomListItemDto>> GetPagedAsync(PagedRequestDto request);                
        Task<List<BomResponseDto>> GetByItemIdAsync(int itemId);               
        Task<bool> SetDefaultAsync(int bomId);
    }
}
