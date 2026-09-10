using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.JobWorkIssueDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IJobWorkIssueService
    {
        Task<JobWorkIssueResponseDto> CreateAsync(CreateJobWorkIssueMainDto dto);
        Task<JobWorkIssueResponseDto?> UpdateAsync(int id, UpdateJobWorkIssueMainDto dto);
        Task<JobWorkIssueResponseDto?> GetByIdAsync(int id);
        Task<PagedResponseDto<JobWorkIssueListDto>> GetPagedAsync(PagedRequestDto request);
        Task<JobWorkIssueResponseDto> ConfirmAsync(int id);
        Task<JobWorkIssueResponseDto> CancelAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}
