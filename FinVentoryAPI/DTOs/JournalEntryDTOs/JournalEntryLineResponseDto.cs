namespace FinVentoryAPI.DTOs.JournalEntryDTOs
{
    public class JournalEntryLineResponseDto
    {
        public int LineId { get; set; }
        public int JournalEntryId { get; set; }

        public int AccountId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string? AccountCode { get; set; }

        public decimal Debit { get; set; }
        public decimal Credit { get; set; }

        public string? Narration { get; set; }
    }
}
