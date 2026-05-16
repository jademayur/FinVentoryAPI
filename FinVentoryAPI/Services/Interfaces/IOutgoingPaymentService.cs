using FinVentoryAPI.DTOs.OutgoingPaymentDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IOutgoingPaymentService
    {
        /// <summary>Returns unpaid / partially-paid purchase bills for a supplier.</summary>
        Task<List<PendingSupplierBillDto>> GetPendingBillsAsync(int businessPartnerId);

        Task<OutgoingPaymentResponseDto> CreateAsync(CreateOutgoingPaymentDto dto);
        Task<bool> UpdateAsync(int id, UpdateOutgoingPaymentDto dto);
        Task<bool> DeleteAsync(int id);

        Task<OutgoingPaymentResponseDto?> GetByIdAsync(int id);
        Task<List<OutgoingPaymentResponseDto>> GetAllAsync();
        Task<PagedResponseDto<OutgoingPaymentResponseDto>> GetPagedAsync(PagedRequestDto request);
    }
}
