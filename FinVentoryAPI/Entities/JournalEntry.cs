using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Entities
{
    public class JournalEntry: BaseEntity
    {
        [Key]
        public int JournalEntryId { get; set; }
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }
        public int AccountId { get; set; }
        public string? EntryNo { get; set; }
        public DateTime EntryDate { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public string? Status { get; set; } 

        public virtual Account? Account { get; set; }
        public virtual ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
    }
}
