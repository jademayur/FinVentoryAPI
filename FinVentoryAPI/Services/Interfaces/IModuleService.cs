using FinVentoryAPI.DTOs.LocationDTOs;
using FinVentoryAPI.DTOs.ModuleDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IModuleService
    {
        Task<ModuleResponseDto> CreateAsync(ModuleCreateDto dto);
        Task<List<ModuleResponseDto>> GetAllAsync();
        Task<ModuleResponseDto?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(int id, ModuleUpdateDto dto);
        Task<bool> DeleteAsync(int id, int userId);
    }
}
