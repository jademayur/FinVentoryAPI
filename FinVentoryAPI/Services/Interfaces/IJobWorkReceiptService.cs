using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.JobWorkReceiptDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IJobWorkReceiptService
    {
        Task<JobWorkReceiptResponseDto> CreateAsync(CreateJobWorkReceiptMainDto dto);
        Task<JobWorkReceiptResponseDto?> UpdateAsync(int id, UpdateJobWorkReceiptMainDto dto);
        Task<JobWorkReceiptResponseDto?> GetByIdAsync(int id);
        Task<PagedResponseDto<JobWorkReceiptListDto>> GetPagedAsync(PagedRequestDto request);
        Task<JobWorkReceiptResponseDto> ConfirmAsync(int id);
        Task<JobWorkReceiptResponseDto> CancelAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}
