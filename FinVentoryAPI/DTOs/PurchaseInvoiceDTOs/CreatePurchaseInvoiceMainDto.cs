using FinVentoryAPI.DTOs.SalesInvoiceDTOs;
using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.PurchaseInvoiceDTOs
{
    public class CreatePurchaseInvoiceMainDto
    {
        [Required(ErrorMessage = "Invoice Date is required.")]
        public DateTime InvoiceDate { get; set; }

        [Required(ErrorMessage = "Due Date is required.")]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Business Partner is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Business Partner.")]
        public int BusinessPartnerId { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Location.")]
        public int LocationId { get; set; }

        // ✅ Purchase Book Account — selected by user
        [Required(ErrorMessage = "Purchase Account is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Purchase Account.")]
        public int PurchaseAccountId { get; set; }

        [Range(-1000, 1000, ErrorMessage = "Round Off must be between -1000 and 1000.")]
        public decimal RoundOff { get; set; } = 0;

        [MaxLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? Remarks { get; set; }

        public int SalesStateCode { get; set; }     // GstState enum int value
        public int BillStateCode { get; set; }      // GstState enum int value
        public int? ContactPersonId { get; set; }    // FK → BusinessPartnerContact.BPContactId
        public int? SalesPersonId { get; set; }      // FK → SalesPerson.SalesPersonId
        [Required(ErrorMessage = "Bill Address is required.")]
        public int BillAddressId { get; set; }      // FK → BusinessPartnerAddress.BPAddressId

        [Required(ErrorMessage = "Ship Address is required.")]
        public int ShipAddressId { get; set; }      // FK → BusinessPartnerAddress.BPAddressId

        public string? TransportName { get; set; }
        public string? VehicleNo { get; set; }
        public string? LrNo { get; set; }
        public DateTime? LrDate { get; set; }


        [Required(ErrorMessage = "At least one item line is required.")]
        [MinLength(1, ErrorMessage = "At least one item line is required.")]
        public List<CreatePurchaseInvoiceDetailDto> Details { get; set; } = new();

    }
}
