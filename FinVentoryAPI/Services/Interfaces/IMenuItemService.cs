using FinVentoryAPI.DTOs.MenuItemDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IMenuItemService
    {
        Task<MenuItemResponseDto> CreateAsync(MenuItemCreateDto dto);
        Task<List<MenuItemResponseDto>> GetAllAsync();
        Task<MenuItemResponseDto?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(int id, MenuItemUpdateDto dto);
        Task<bool> DeleteAsync(int id, int userId);
    }
}
