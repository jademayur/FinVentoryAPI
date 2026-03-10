 using FinVentoryAPI.DTOs.AccountGroupDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;


namespace FinVentoryAPI.Services.Interfaces
{
    public interface IAccountGroupService
    {
        Task<AccountGroupResponseDto> CreateAsync(CreateAccountGroupDto dto);

        Task<List<AccountGroupResponseDto>> GetAllAsync();

        Task<AccountGroupResponseDto?> GetByIdAsync(int id);

        Task<bool> UpdateAsync(int id, UpdateAccountGroupDto dto);

        Task<bool> DeleteAsync(int id);

        Task<PagedResponseDto<AccountGroupResponseDto>> GetPagedAsync(PagedRequestDto request);
    }
}
