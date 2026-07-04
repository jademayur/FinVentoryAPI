using FinVentoryAPI.DTOs.GRNDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IGRNService
    {
        // ── CRUD ─────────────────────────────────────────────────────────────
        Task<GRNResponseDto> CreateAsync(CreateGRNMainDto dto);
        Task<bool> UpdateAsync(int id, UpdateGRNMainDto dto);
        Task<bool> DeleteAsync(int id);

        // ── Workflow ─────────────────────────────────────────────────────────
        Task<bool> ConfirmAsync(int id);
        Task<bool> CancelAsync(int id);

        // ── Queries ───────────────────────────────────────────────────────────
        Task<List<GRNResponseDto>> GetAllAsync();
        Task<GRNResponseDto?> GetByIdAsync(int id);
        Task<PagedResponseDto<GRNResponseDto>> GetPagedAsync(PagedRequestDto request);

        // ── Helper endpoints for the GRN entry form ──────────────────────────
               
        /// Returns confirmed Purchase Orders for a supplier that still have
        /// at least one line with pending (un-received) quantity.
        /// Used to populate the PO picker dropdown.
        Task<List<PurchaseOrderPickerDto>> GetPurchaseOrdersForSupplierAsync(int businessPartnerId);

       
        /// Given one or more selected PO IDs, returns pre-filled header and
        /// detail data ready to bind into the GRN entry form.
        Task<GRNPrefillDto> GetGRNPrefillAsync(List<int> purchaseOrderIds);
    }
}
