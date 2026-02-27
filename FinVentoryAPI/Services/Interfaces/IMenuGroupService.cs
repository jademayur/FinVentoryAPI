using FinVentoryAPI.DTOs.MenuGroupDTOs;
using FinVentoryAPI.DTOs.ModuleDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IMenuGroupService
    {
        Task<List<MenuGroupResponseDto>> GetAllAsync();
        Task<MenuGroupResponseDto?> GetByIdAsync(int id);
    }
}
    