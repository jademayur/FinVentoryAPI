namespace FinVentoryAPI.DTOs.AuditLogDTOs
{
    public class AuditLogResponseDto
    {
        public long AuditLogId { get; set; }
        public int CompanyId { get; set; }
        public int? FinancialYearId { get; set; }
        public int? UserId { get; set; }
        public string Module { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public int? EntityId { get; set; }
        public string? EntityNo { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? Remarks { get; set; }
        public string? IpAddress { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
