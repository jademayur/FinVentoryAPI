using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.ProductionIssueDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IProductionIssueService
    {
        Task<ProductionIssueResponseDto> CreateAsync(CreateProductionIssueMainDto dto);
        Task<ProductionIssueResponseDto?> UpdateAsync(int id, UpdateProductionIssueMainDto dto);
        Task<ProductionIssueResponseDto?> GetByIdAsync(int id);
        Task<PagedResponseDto<ProductionIssueListDto>> GetPagedAsync(PagedRequestDto request);
        Task<ProductionIssueResponseDto> ConfirmAsync(int id);
        Task<ProductionIssueResponseDto> CancelAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}
