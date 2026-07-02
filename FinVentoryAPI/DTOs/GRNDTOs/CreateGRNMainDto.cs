using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.GRNDTOs
{
    public class CreateGRNMainDto
    {
        [Required]
        public DateTime GRNDate { get; set; }

        public string? SupplierInvoiceNo { get; set; }
        public DateTime? SupplierInvoiceDate { get; set; }
        public string? RefNo { get; set; }
        public DateTime? RefDate { get; set; }
        public string? Remarks { get; set; }

        [Required]
        public int BusinessPartnerId { get; set; }          // Supplier

        [Required]
        public int LocationId { get; set; }

        public int? ContactPersonId { get; set; }       
        public int? BillAddressId { get; set; }
        public int? ShipAddressId { get; set; }
        public int? PurchaseStateCode { get; set; }
        public int? BillStateCode { get; set; }
        public decimal RoundOff { get; set; }

        [Required, MinLength(1)]
        public List<CreateGRNDetailDto> Details { get; set; } = new();
    }
}
