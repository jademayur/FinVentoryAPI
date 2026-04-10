using Microsoft.EntityFrameworkCore.Migrations;

namespace FinVentoryAPI.DTOs.SalesInvoiceDTOs
{
    public class SalesInvoiceResponseDto
    {
        public int InvoiceId { get; set; }
        public int FinYearId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;

        // Business Partner
        public int BusinessPartnerId { get; set; }
        public string BusinessPartnerName { get; set; } = string.Empty;
        public string BusinessPartnerCode { get; set; } = string.Empty;

        // Location
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;

        // ✅ Sales Book Account — stored in invoice
        public int SalesAccountId { get; set; }
        public string SalesAccountName { get; set; } = string.Empty;

        // ✅ Receivable Account — from BusinessPartner (not stored in invoice)
        public int ReceivableAccountId { get; set; }

        // ── New Fields ──────────────────────────────────────
        public int? SalesStateCode { get; set; }
        public string? SalesStateName { get; set; }

        public int? BillStateCode { get; set; }
        public string? BillStateName { get; set; }

        public int? ContactPersonId { get; set; }
        public string? ContactPersonName { get; set; }
        public string? ContactPersonMobile { get; set; }

        public int? SalesPersonId { get; set; }
        public string? SalesPersonName { get; set; }

        public int? BillAddressId { get; set; }
        public string? BillAddressLine { get; set; }

        public int? ShipAddressId { get; set; }
        public string? ShipAddressLine { get; set; }

        // Totals
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal RoundOff { get; set; }
        public decimal NetTotal { get; set; }

        public string? Remarks { get; set; }

        // Lines
        public List<SalesInvoiceDetailResponseDto> Details { get; set; } = new();
        public List<SalesInvoiceTaxDetailResponseDto> TaxDetails { get; set; } = new();

        // Audit
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? TransportName { get; set; }
        public string? VehicleNo { get; set; }
        public string? LrNo { get; set; }
        public DateTime? LrDate { get; set; }

        public decimal Amount { get; set; }   // = SubTotal (taxable base)
        public decimal Discount { get; set; }   // = sum of all line discounts
        public decimal NetAmount { get; set; }   // = NetTotal
    }
}


