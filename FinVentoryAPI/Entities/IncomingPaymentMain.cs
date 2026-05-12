using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Entities
{
    public class IncomingPaymentMain 
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

        /// <summary>Bank or Cash GL account that receives the money.</summary>
        public int DepositAccountId { get; set; }

        /// <summary>Cash | Cheque | NEFT | RTGS | UPI | Other</summary>
        public string PaymentMode { get; set; } = "Cash";
        public string? ChequeNo { get; set; }
        public DateTime? ChequeDate { get; set; }
        public string? BankName { get; set; }
        public string? TransactionRef { get; set; }
        public string? Remarks { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal AllocatedAmount { get; set; }

        /// <summary>Amount not tied to any specific invoice (advance/on-account).</summary>
        public decimal OnAccountAmount { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        // ── Use int? to match the pattern in SalesInvoiceMain ──────
        // If your DB stores these as bigint, change to long? here
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // ── Navigation ─────────────────────────────────────────────
        public BusinessPartner? BusinessPartner { get; set; }
        public Account? DepositAccount { get; set; }
        public List<IncomingPaymentAllocation>? Allocations { get; set; }
    }
}
