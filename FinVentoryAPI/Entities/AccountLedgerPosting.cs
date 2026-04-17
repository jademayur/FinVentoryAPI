using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Entities
{
    public class AccountLedgerPosting : BaseEntity
    {
        [Key]
        public int PostingId { get; set; }
        public int CompanyId { get; set; }
        public int FinancialYearId { get; set; }
        public int AccountId { get; set; }
        public int? BusinessPartnerId { get; set; }
        public DateTime Date { get; set; }
        public string VoucherType { get; set; } = string.Empty;
        public string VoucherNo { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string? Remarks { get; set; }

        public Account? Account { get; set; }
        public BusinessPartner? BusinessPartner { get; set; }
    }
}
