using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.PurchaseInvoiceDTOs
{
    public class PurchaseInvoiceSerialCreateDto
    {
        [Required(ErrorMessage = "Serial No is required.")]
        [MaxLength(100, ErrorMessage = "Serial No cannot exceed 100 characters.")]
        public string SerialNo { get; set; } = string.Empty;

        public DateTime? PurchaseDate { get; set; }
        public DateTime? WarrantyExpiry { get; set; }
    }
}
