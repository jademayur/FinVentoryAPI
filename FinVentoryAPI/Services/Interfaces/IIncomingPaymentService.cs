using FinVentoryAPI.DTOs.IncomingPaymentDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IIncomingPaymentService
    {
        Task<List<PendingBillDto>> GetPendingBillsAsync(int businessPartnerId);

        Task<IncomingPaymentResponseDto> CreateAsync(CreateIncomingPaymentDto dto);
        Task<bool> UpdateAsync(int id, UpdateIncomingPaymentDto dto);
        Task<bool> DeleteAsync(int id);

        Task<IncomingPaymentResponseDto?> GetByIdAsync(int id);
        Task<List<IncomingPaymentResponseDto>> GetAllAsync();
        Task<PagedResponseDto<IncomingPaymentResponseDto>> GetPagedAsync(PagedRequestDto request);
    }
}
