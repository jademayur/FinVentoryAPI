using FinVentoryAPI.DTOs.UserDTOs;
using FinVentoryAPI.Entities;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> GetByIdAsync(int id);
        Task<string> CreateAsync(UserCreateDto dto);
        Task<string> UpdateAsync(UserUpdateDto dto);
        Task<string> DeleteAsync(int id);
    }
}
