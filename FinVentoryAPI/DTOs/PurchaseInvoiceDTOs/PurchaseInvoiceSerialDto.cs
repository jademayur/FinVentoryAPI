using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.PurchaseInvoiceDTOs
{
    public class PurchaseInvoiceSerialDto
    {
        [Required(ErrorMessage = "Serial is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Serial.")]
        public int SerialId { get; set; }
    }
}
