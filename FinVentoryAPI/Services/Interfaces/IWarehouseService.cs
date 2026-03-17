using FinVentoryAPI.DTOs.AccountGroupDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.WarehouseDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IWarehouseService
    {
        Task<WarehouseResponseDto> CreateAsync(CreateWarehouseDto dto);

        Task<List<WarehouseResponseDto>> GetAllAsync();

        Task<WarehouseResponseDto?> GetByIdAsync(int id);

        Task<bool> UpdateAsync(int id, UpdateWarehouseDto dto);

        Task<bool> DeleteAsync(int id);

        Task<PagedResponseDto<WarehouseResponseDto>> GetPagedAsync(PagedRequestDto request);
    }
}
