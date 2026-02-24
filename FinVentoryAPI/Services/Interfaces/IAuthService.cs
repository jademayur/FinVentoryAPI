using FinVentoryAPI.DTOs.CompanySelectionDtos;
using FinVentoryAPI.DTOs.LoginDtos;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<object> LoginAsync(LoginDto dto);
        Task<string> GenerateTokenAsync(CompanySelectionDto dto);
    }
}
