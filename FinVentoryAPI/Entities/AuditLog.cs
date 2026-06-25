namespace FinVentoryAPI.Entities
{
    public class AuditLog
    {
        public long AuditLogId { get; set; }
        public int CompanyId { get; set; }
        public int? UserId { get; set; }
        public string Module { get; set; } = string.Empty;   // "SalesQuotation"
        public string Action { get; set; } = string.Empty;   // "Create","Update","Delete","Revise","Copy"
        public int? EntityId { get; set; }                   // e.g. QuotationId
        public string? EntityNo { get; set; }                // e.g. "QT-2526-0001"
        public string? OldValues { get; set; }               // JSON snapshot before
        public string? NewValues { get; set; }               // JSON snapshot after
        public string? Remarks { get; set; }
        public string? IpAddress { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
