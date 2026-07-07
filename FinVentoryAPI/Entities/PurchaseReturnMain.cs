using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Entities
{
    public class PurchaseReturnMain
    {
        [Key]
        public int ReturnId { get; set; }
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }
        public string ReturnNo { get; set; } = string.Empty;
        public DateTime ReturnDate { get; set; }

        public int? OriginalInvoiceId { get; set; }
        public string? OriginalInvoiceNo { get; set; }
        public DateTime? OriginalInvoiceDate { get; set; }

        public string NoteType { get; set; } = "Debit"; // Debit note to supplier

        public int BusinessPartnerId { get; set; }
        public int LocationId { get; set; }
        public int PurchaseAccountId { get; set; }

        public int? PurchaseStateCode { get; set; }
        public int? BillStateCode { get; set; }
        public int? BillAddressId { get; set; }

        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal RoundOff { get; set; }
        public decimal NetTotal { get; set; }

        public string Status { get; set; } = "Draft";
        public string? Remarks { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; } = true;
        public long? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation
        public Company? Company { get; set; }
        public BusinessPartner? BusinessPartner { get; set; }
        public Location? Location { get; set; }
        public Account? PurchaseAccount { get; set; }
        public BusinessPartnerAddress? BillAddress { get; set; }
        public PurchaseInvoiceMain? OriginalInvoice { get; set; }

        public List<PurchaseReturnDetail>? Details { get; set; }
        public List<PurchaseReturnTaxDetail>? TaxDetails { get; set; }
    }
}
