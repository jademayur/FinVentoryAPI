namespace FinVentoryAPI.DTOs.JournalEntryDTOs
{
    public class JournalEntryResponseDto
    {
        public int JournalEntryId { get; set; }
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }

        public int AccountId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string? AccountCode { get; set; }

        public string? EntryNo { get; set; }
        public DateOnly EntryDate { get; set; }

        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }

        public string Status { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public List<JournalEntryLineResponseDto> Lines { get; set; } = new();
    }
}
