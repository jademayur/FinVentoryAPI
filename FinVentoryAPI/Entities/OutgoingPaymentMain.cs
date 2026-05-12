using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Entities
{
    public class OutgoingPaymentMain
    {
        [Key]
        public int PaymentId { get; set; }
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }
        public string PaymentNo { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }

        /// <summary>Draft | Confirmed | Cancelled</summary>
        public string Status { get; set; } = "Draft";

        public int BusinessPartnerId { get; set; }

        /// <summary>Bank or Cash GL account from which payment is made.</summary>
        public int PaymentAccountId { get; set; }

        /// <summary>Cash | Cheque | NEFT | RTGS | UPI | Other</summary>
        public string PaymentMode { get; set; } = "Cash";
        public string? ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; }
        public string? BankName { get; set; }
        public string? TransactionRef { get; set; }
        public string? Remarks { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal AllocatedAmount { get; set; }

        /// <summary>Amount not tied to any specific bill (advance to supplier).</summary>
        public decimal OnAccountAmount { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // ── Navigation ─────────────────────────────────────────────
        public BusinessPartner? BusinessPartner { get; set; }
        public Account? PaymentAccount { get; set; }
        public List<OutgoingPaymentAllocation>? Allocations { get; set; }
    }
}
