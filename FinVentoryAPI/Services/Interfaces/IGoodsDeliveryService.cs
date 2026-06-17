using FinVentoryAPI.DTOs.GoodsDeliveryDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.DTOs.SalesInvoiceDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface IGoodsDeliveryService
    {
        // ── CRUD ─────────────────────────────────────────────────────────────────
        Task<GoodsDeliveryResponseDto> CreateAsync(CreateGoodsDeliveryMainDto dto);
        Task<bool> UpdateAsync(int id, UpdateGoodsDeliveryMainDto dto);
        Task<bool> DeleteAsync(int id);

        // ── Status transitions ────────────────────────────────────────────────────
        Task<bool> ConfirmAsync(int id);
        Task<bool> CancelAsync(int id);

        // ── Queries ───────────────────────────────────────────────────────────────
        Task<List<GoodsDeliveryResponseDto>> GetAllAsync();
        Task<GoodsDeliveryResponseDto?> GetByIdAsync(int id);
        Task<PagedResponseDto<GoodsDeliveryResponseDto>> GetPagedAsync(PagedRequestDto request);

        // ── Picker / prefill helpers ──────────────────────────────────────────────

        /// <summary>
        /// Returns Confirmed orders (with pending qty) for a given customer.
        /// Used to populate the "select orders" picker before opening the delivery form.
        /// </summary>
        Task<List<OrderPickerDto>> GetOrdersForCustomerAsync(int businessPartnerId);

        /// <summary>
        /// Prefills the delivery form from one or more selected sales order IDs.
        /// Merges lines from all orders; each line shows orderedQty, deliveredQty, pendingQty.
        /// </summary>
        Task<DeliveryPrefillDto> GetDeliveryPrefillAsync(List<int> orderIds);

        Task<List<DeliveryPickerDto>> GetDeliveriesForCustomerAsync(int businessPartnerId);
    }
}
