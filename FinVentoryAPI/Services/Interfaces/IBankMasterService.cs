using FinVentoryAPI.DTOs.BankDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IBankMasterService
    {
        Task<BankMasterResponseDto> CreateAsync(CreateBankMasterDto dto);

        Task<List<BankMasterResponseDto>> GetAllAsync();

        Task<BankMasterResponseDto?> GetByIdAsync(int id);

        Task<bool> UpdateAsync(int id, UpdateBankMasterDto dto);

        Task<bool> DeleteAsync(int id);
    }
}
