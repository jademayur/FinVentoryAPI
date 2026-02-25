using FinVentoryAPI.DTOs.UserDTOs;
using FinVentoryAPI.Entities;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserResponseDto> CreateAsync(UserCreateDto dto);

        Task<List<UserResponseDto>> GetAllAsync();

        Task<UserResponseDto?> GetByIdAsync(int id);

        Task<bool> UpdateAsync(int id, UserUpdateDto dto);

        Task<bool> DeleteAsync(int id);
    }
}
