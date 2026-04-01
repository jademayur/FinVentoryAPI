using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.SalesInvoiceDTOs
{
    public class CreateSalesInvoiceMainDto
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

        // ✅ Sales Book Account — selected by user
        [Required(ErrorMessage = "Sales Account is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Sales Account.")]
        public int SalesAccountId { get; set; }

        [Range(-1000, 1000, ErrorMessage = "Round Off must be between -1000 and 1000.")]
        public decimal RoundOff { get; set; } = 0;

        [MaxLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? Remarks { get; set; }

        [Required(ErrorMessage = "At least one item line is required.")]
        [MinLength(1, ErrorMessage = "At least one item line is required.")]
        public List<CreateSalesInvoiceDetailDto> Details { get; set; } = new();
    }
}