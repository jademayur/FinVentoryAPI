using FinVentoryAPI.DTOs.LocationDTOs;
using FinVentoryAPI.DTOs.MenuItemDTOs;
using FinVentoryAPI.DTOs.RoleRightsDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IRoleRightService
    {
        Task<RoleRightResponseDto> CreateAsync(RoleRightCreateDto dto);

        Task<List<RoleRightResponseDto>> GetAllAsync();

        Task<RoleRightResponseDto?> GetByIdAsync(int id);

        Task<bool> UpdateAsync(int id, RoleRightUpdateDto dto);

        Task<bool> DeleteAsync(int id, int userId);

        Task<List<MenuItemResponseDto>> GetMenuByRoleAsync(int roleId);

        Task<FormPermissionDto> GetFormPermissionsAsync(int MenuItemID , int roleId);

    }
}
