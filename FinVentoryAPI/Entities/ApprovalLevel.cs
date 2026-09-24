namespace FinVentoryAPI.Entities
{
    public class ApprovalLevel
    {
        public int ApprovalLevelId { get; set; }
        public int CompanyId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public int LevelNumber { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public decimal? MaxAmount { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
