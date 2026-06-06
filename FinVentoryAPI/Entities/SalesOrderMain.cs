using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Entities
{
    public class SalesOrderMain : BaseEntity
    {
        [Key]
        public int OrderId { get; set; }
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }

        public string OrderNo { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }

        // Reference to quotation (optional)
        public int? QuotationId { get; set; }
        public string? QuotationNo { get; set; }
        public DateTime? QuotationDate { get; set; }
        public string? RefNo { get; set; }
        public DateTime? RefDate { get; set; }

        public int BusinessPartnerId { get; set; }
        public int LocationId { get; set; }

        public int? ContactPersonId { get; set; }
        public int? SalesPersonId { get; set; }
        public int? BillAddressId { get; set; }
        public int? ShipAddressId { get; set; }

        public int? SalesStateCode { get; set; }
        public int? BillStateCode { get; set; }

        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal RoundOff { get; set; }
        public decimal NetTotal { get; set; }

        public string Status { get; set; } = "Draft"; // Draft, Confirmed, Cancelled
        public string? Remarks { get; set; }

        // Navigation
        public Company? Company { get; set; }
        public BusinessPartner? BusinessPartner { get; set; }
        public Location? Location { get; set; }
        public BusinessPartnerContact? ContactPerson { get; set; }
        public SalesPerson? SalesPerson { get; set; }
        public BusinessPartnerAddress? BillAddress { get; set; }
        public BusinessPartnerAddress? ShipAddress { get; set; }
        public SalesQuotationMain? Quotation { get; set; }

        public List<SalesOrderDetail>? Details { get; set; }
        public List<SalesOrderTaxDetail>? TaxDetails { get; set; }
    }
}
