using FinVentoryAPI.Enums;

namespace FinVentoryAPI.Entities
{
    public class DocumentCopyLog
    {
        public int CopyLogId { get; set; }   // PK

        // ── Source (the document being copied FROM) ───────────
        public DocumentType SourceType { get; set; }
        public int SourceId { get; set; }   // e.g. SalesOrderMain.OrderId
        public int? SourceDetailId { get; set; }   // e.g. SalesOrderDetail.DetailId (null = header-level copy)
        public decimal SourceQty { get; set; }   // original qty on source line

        // ── Target (the document being copied INTO) ───────────
        public DocumentType TargetType { get; set; }
        public int TargetId { get; set; }   // e.g. SalesInvoiceMain.InvoiceId
        public int? TargetDetailId { get; set; }   // e.g. SalesInvoiceDetail.DetailId

        // ── Copy metadata ─────────────────────────────────────
        public decimal CopiedQty { get; set; }   // qty actually copied (may be partial)
        public int ItemId { get; set; }   // denormalized for quick pending-qty queries
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public int CreatedBy { get; set; }

        // ── Soft delete (when target document is deleted) ─────
        public bool IsDeleted { get; set; } = false;
    }
}
