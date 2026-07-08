using FinVentoryAPI.DTOs.CopyDocumentDTOs;

namespace FinVentoryAPI.Services.Interfaces
{
    public interface ICopyDocumentService
    {
        // ── Sales flows ───────────────────────────────────────

        /// <summary>
        /// Sales Order → Sales Invoice
        /// Copies pending (not yet invoiced) lines from a sales order.
        /// </summary>
        Task<CopiedSalesInvoiceDto> CopySalesOrderToInvoiceAsync(
            int salesOrderId,
            CopyRequestDto? request = null);

        /// <summary>
        /// Sales Invoice → Sales Return (Credit Note)
        /// Copies pending (not yet returned) lines from a sales invoice.
        /// </summary>
        Task<CopiedSalesReturnDto> CopySalesInvoiceToReturnAsync(
            int salesInvoiceId,
            CopyRequestDto? request = null);

        /// <summary>
        /// Quotation → Sales Order
        /// Copies pending lines from a quotation.
        /// </summary>
        Task<CopiedSalesOrderDto> CopyQuotationToSalesOrderAsync(
            int quotationId,
            CopyRequestDto? request = null);

        // ── Purchase flows ────────────────────────────────────

        /// <summary>
        /// Purchase Order → Purchase Invoice
        /// Copies pending (not yet invoiced) lines from a purchase order.
        /// </summary>
        Task<CopiedPurchaseInvoiceDto> CopyPurchaseOrderToInvoiceAsync(
            int purchaseOrderId,
            CopyRequestDto? request = null);

        /// <summary>
        /// Purchase Invoice → Purchase Return (Debit Note)
        /// Copies pending (not yet returned) lines from a purchase invoice.
        /// </summary>
        Task<CopiedPurchaseReturnDto> CopyPurchaseInvoiceToReturnAsync(
            int purchaseInvoiceId,
            CopyRequestDto? request = null);

        // ── Copy Log management ───────────────────────────────

        /// <summary>
        /// Persists the copy log entries AFTER the target document has been saved.
        /// Called internally by Create methods in SalesInvoiceService etc.
        /// </summary>
        Task SaveCopyLogAsync(SaveCopyLogDto dto);

        /// <summary>
        /// Soft-deletes copy log entries when a target document is deleted.
        /// Called internally by Delete methods.
        /// </summary>
        Task SoftDeleteCopyLogAsync(
            FinVentoryAPI.Enums.DocumentType targetType,
            int targetId);
    }
}
