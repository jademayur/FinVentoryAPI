using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Entities
{
    public class GoodsDeliveryMain
    {
        [Key]
        public int DeliveryId { get; set; }
        public int CompanyId { get; set; }
        public int FinYearId { get; set; }

        public string DeliveryNo { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; }
        public string? RefNo { get; set; }
        public DateTime? RefDate { get; set; }
        public string? Remarks { get; set; }

        /// <summary>Draft | Confirmed | Cancelled</summary>
        public string Status { get; set; } = "Draft";

        // ── Header references ────────────────────────────────────
        public int BusinessPartnerId { get; set; }
        public int LocationId { get; set; }
        public int? ContactPersonId { get; set; }
        public int? SalesPersonId { get; set; }
        public int? BillAddressId { get; set; }
        public int? ShipAddressId { get; set; }
        public int? SalesStateCode { get; set; }
        public int? BillStateCode { get; set; }

        // ── Totals ───────────────────────────────────────────────
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CessAmount { get; set; }
        public decimal RoundOff { get; set; }
        public decimal NetTotal { get; set; }

        // ── Audit ────────────────────────────────────────────────
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // ── Navigation ───────────────────────────────────────────
        public BusinessPartner? BusinessPartner { get; set; }
        public Location? Location { get; set; }
        public BusinessPartnerContact? ContactPerson { get; set; }
        public SalesPerson? SalesPerson { get; set; }
        public BusinessPartnerAddress? BillAddress { get; set; }
        public BusinessPartnerAddress? ShipAddress { get; set; }

        public List<GoodsDeliveryDetail>? Details { get; set; }
        public List<GoodsDeliveryTaxDetail>? TaxDetails { get; set; }

        /// <summary>Links to the source sales orders (one delivery can span many orders).</summary>
        
    }
}
