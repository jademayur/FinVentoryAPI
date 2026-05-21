using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Entities
{
    public class JournalEntryLine
    {
        [Key]
        public int LineId { get; set; }
        public int JournalEntryId { get; set; }
        public int AccountId { get; set; }
        public string? AccountCode { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string? Narration { get; set; }

        public virtual JournalEntry? JournalEntry { get; set; }
        public virtual Account? Account { get; set; }
    }
}
