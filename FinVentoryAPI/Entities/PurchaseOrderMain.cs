using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class PurchaseOrderMain : BaseEntity
    {
        [Key]        
        public int OrderId { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        public int FinYearId { get; set; }

        [Required]
        [MaxLength(30)]
        public string OrderNo { get; set; } = string.Empty;

        [Required]
        public DateTime OrderDate { get; set; }

        public DateTime? DeliveryDate { get; set; }

        // Reference fields
        [MaxLength(50)]
        public string? RefNo { get; set; }

        public DateTime? RefDate { get; set; }

        // Business Partner
        [Required]
        public int BusinessPartnerId { get; set; }

        [ForeignKey(nameof(BusinessPartnerId))]
        public BusinessPartner? BusinessPartner { get; set; }

        // Location
        [Required]
        public int LocationId { get; set; }

        [ForeignKey(nameof(LocationId))]
        public Location? Location { get; set; }

        // State codes (GST)
        public int? PurchaseStateCode { get; set; }
        public int? BillStateCode { get; set; }

        // Contact & Purchase Person
        public int? ContactPersonId { get; set; }

        [ForeignKey(nameof(ContactPersonId))]
        public BusinessPartnerContact? ContactPerson { get; set; }
       

        // Addresses
        public int? BillAddressId { get; set; }

        [ForeignKey(nameof(BillAddressId))]
        public BusinessPartnerAddress? BillAddress { get; set; }

        public int? ShipAddressId { get; set; }

        [ForeignKey(nameof(ShipAddressId))]
        public BusinessPartnerAddress? ShipAddress { get; set; }

        // Financials
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CessAmount { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal RoundOff { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetTotal { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        // Status: Draft | Confirmed | Cancelled
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Draft";

      

        // Navigation
        public List<PurchaseOrderDetail>? Details { get; set; }
        public List<PurchaseOrderTaxDetail>? TaxDetails { get; set; }
    }
}
