    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace FinVentoryAPI.Entities
    {
        public class SalesInvoiceMain : BaseEntity
        {
            [Key]
            public int InvoiceId { get; set; }
            public int CompanyId { get; set; }
            public int FinYearId { get; set; }

            // Invoice Info
            public string InvoiceNo { get; set; } = string.Empty;  // Auto-generated
            public DateTime InvoiceDate { get; set; }
            public DateTime DueDate { get; set; }

            public int BusinessPartnerId { get; set; }
            public int LocationId { get; set; }

            // Receivable account — from BusinessPartner.AccountId
            // Not stored here — fetched from BP when posting journal
            // ✅ Only Sales Book Account stored
            public int SalesAccountId { get; set; }

            public decimal SubTotal { get; set; }       // Sum of all TaxableAmount
            public decimal TaxAmount { get; set; }       // Sum of IGST+CGST+SGST
            public decimal CessAmount { get; set; }      // Sum of all Cess
            public decimal RoundOff { get; set; }        // +/- rounding
            public decimal NetTotal { get; set; }        // SubTotal + TaxAmount + CessAmount + RoundOff

            public string? Remarks { get; set; }
            public string Status { get; set; } = "Draft";

            public int? SalesStateCode { get; set; }     // GstState enum int value
            public int? BillStateCode { get; set; }      // GstState enum int value
            public int? ContactPersonId { get; set; }    // FK → BusinessPartnerContact.BPContactId
            public int? SalesPersonId { get; set; }      // FK → SalesPerson.SalesPersonId
            public int? BillAddressId { get; set; }      // FK → BusinessPartnerAddress.BPAddressId
            public int? ShipAddressId { get; set; }      // FK → BusinessPartnerAddress.BPAddressId


        // Navigation
        public BusinessPartner? BusinessPartner { get; set; }
            public Location? Location { get; set; }

            [ForeignKey(nameof(SalesAccountId))]
            public Account? SalesAccount { get; set; }
            public ICollection<SalesInvoiceDetail>? Details { get; set; }
            public ICollection<SalesInvoiceTaxDetail>? TaxDetails { get; set; }

        }
    }
