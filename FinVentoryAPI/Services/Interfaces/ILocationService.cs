using FinVentoryAPI.DTOs.LocationDTOs;
using FinVentoryAPI.DTOs.UserDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface ILocationService
    {
        Task<LocationResponseDto> CreateAsync(CreateLocationDTO dto, int userId);

        Task<List<LocationResponseDto>> GetAllAsync();

        Task<LocationResponseDto?> GetByIdAsync(int id);

        Task<bool> UpdateAsync(int id, UpdateLocationDTO dto, int userId);

        Task<bool> DeleteAsync(int id, int userId);
    }
}
