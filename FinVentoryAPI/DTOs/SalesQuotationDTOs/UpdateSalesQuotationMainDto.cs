using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.SalesQuotationDTOs
{
    public class UpdateSalesQuotationMainDto
    {
        [Required(ErrorMessage = "Quotation Date is required.")]
        public DateTime QuotationDate { get; set; }

        public DateTime? ValidUntilDate { get; set; }

        [Required(ErrorMessage = "Business Partner is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Business Partner.")]
        public int BusinessPartnerId { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Location.")]
        public int LocationId { get; set; }

        [Range(-1000, 1000, ErrorMessage = "Round Off must be between -1000 and 1000.")]
        public decimal RoundOff { get; set; } = 0;

        [MaxLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? Remarks { get; set; }

        public int? SalesStateCode { get; set; }
        public int? BillStateCode { get; set; }

        public int? ContactPersonId { get; set; }
        public int? SalesPersonId { get; set; }

        [Required(ErrorMessage = "Bill Address is required.")]
        public int BillAddressId { get; set; }

        [Required(ErrorMessage = "Ship Address is required.")]
        public int ShipAddressId { get; set; }

        [Required(ErrorMessage = "At least one item line is required.")]
        [MinLength(1, ErrorMessage = "At least one item line is required.")]
        public List<UpdateSalesQuotationDetailDto> Details { get; set; } = new();
    }
}
