namespace FinVentoryAPI.DTOs.ApprovalDTOs
{
    public class ApprovalLevelDto
    {
        public int ApprovalLevelId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public int LevelNumber { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public decimal? MaxAmount { get; set; }
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpsertApprovalLevelDto
    {
        public string DocumentType { get; set; } = string.Empty;
        public int LevelNumber { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public decimal? MaxAmount { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class BulkUpsertApprovalLevelDto
    {
        public string DocumentType { get; set; } = string.Empty;
        public List<UpsertApprovalLevelDto> Levels { get; set; } = new();
    }

    public class ApprovalActionDto
    {
        public string DocumentType { get; set; } = string.Empty;
        public int DocumentId { get; set; }
        public string? Remarks { get; set; }
    }

    public class ApprovalQueueDto
    {
        public string DocumentType { get; set; } = string.Empty;
        public int DocumentId { get; set; }
        public string DocumentNo { get; set; } = string.Empty;
        public decimal? NetTotal { get; set; }
        public int CurrentLevel { get; set; }
        public string CurrentLevelName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    public class ApprovalHistoryDto
    {
        public string DocumentType { get; set; } = string.Empty;
        public int DocumentId { get; set; }
        public List<ApprovalLogEntryDto> Logs { get; set; } = new();
    }

    public class ApprovalLogEntryDto
    {
        public int LevelNumber { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public DateTime ActionDate { get; set; }
    }
}
