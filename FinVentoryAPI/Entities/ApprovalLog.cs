namespace FinVentoryAPI.Entities
{
    public class ApprovalLog
    {
        public int ApprovalLogId { get; set; }
        public int CompanyId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public int DocumentId { get; set; }
        public int LevelNumber { get; set; }
        public string Action { get; set; } = string.Empty; // Submit, Approve, Reject
        public int UserId { get; set; }
        public string? Remarks { get; set; }
        public DateTime ActionDate { get; set; } = DateTime.UtcNow;
    }
}
