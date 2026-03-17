using FinVentoryAPI.DTOs.ItemGroupDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IItemGroupService
    {
        Task<ItemGroupResponseDto> CreateAsync(CreateItemGroupDto dto);
        Task<bool> UpdateAsync(int id,UpdateItemGroupDto dto);
        Task<bool> DeleteAsync(int id);
        Task<List<ItemGroupResponseDto>> GetAllAsync();
        Task<ItemGroupResponseDto> GetByIdAsync(int id);
        Task<PagedResponseDto<ItemGroupResponseDto>> GetPagedAsync(PagedRequestDto request);

    }
}
