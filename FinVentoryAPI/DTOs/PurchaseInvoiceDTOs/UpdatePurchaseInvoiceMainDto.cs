using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.PurchaseInvoiceDTOs
{
    public class UpdatePurchaseInvoiceMainDto
    {
        [Required(ErrorMessage = "Invoice Date is required.")]
        public DateTime InvoiceDate { get; set; }

        [Required(ErrorMessage = "Supplier Invoice No is required.")]
        [MaxLength(100, ErrorMessage = "Supplier Invoice No cannot exceed 100 characters.")]
        public string SupplierInvoiceNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Supplier Invoice Date is required.")]
        public DateTime SupplierInvoiceDate { get; set; }

        [Required(ErrorMessage = "Due Date is required.")]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Business Partner (Supplier) is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Business Partner.")]
        public int BusinessPartnerId { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Location.")]
        public int LocationId { get; set; }

        [Required(ErrorMessage = "Purchase Account is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Purchase Account.")]
        public int PurchaseAccountId { get; set; }

        [Range(-1000, 1000, ErrorMessage = "Round Off must be between -1000 and 1000.")]
        public decimal RoundOff { get; set; } = 0;

        [MaxLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? Remarks { get; set; }

        public int PurchaseStateCode { get; set; }
        public int BillStateCode { get; set; }

        public int? ContactPersonId { get; set; }

        [Required(ErrorMessage = "Bill Address is required.")]
        public int BillAddressId { get; set; }

        [Required(ErrorMessage = "Ship Address is required.")]
        public int ShipAddressId { get; set; }

        [MaxLength(200, ErrorMessage = "Transport Name cannot exceed 200 characters.")]
        public string? TransportName { get; set; }

        [MaxLength(50, ErrorMessage = "Vehicle No cannot exceed 50 characters.")]
        public string? VehicleNo { get; set; }

        [MaxLength(100, ErrorMessage = "LR No cannot exceed 100 characters.")]
        public string? LrNo { get; set; }

        public DateTime? LrDate { get; set; }

        [Required(ErrorMessage = "At least one item line is required.")]
        [MinLength(1, ErrorMessage = "At least one item line is required.")]
        public List<UpdatePurchaseInvoiceDetailDto> Details { get; set; } = new();
    }
}
