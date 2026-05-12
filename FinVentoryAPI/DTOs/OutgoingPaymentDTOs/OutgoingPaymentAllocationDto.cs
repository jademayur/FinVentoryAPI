using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.OutgoingPaymentDTOs
{
    public class OutgoingPaymentAllocationDto
    {
        [Required(ErrorMessage = "Bill ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Bill ID must be greater than zero.")]
        public int BillId { get; set; }

        [Required(ErrorMessage = "Bill number is required.")]
        [StringLength(50, ErrorMessage = "Bill number cannot exceed 50 characters.")]
        public string BillNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Amount applied is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount applied must be greater than zero.")]
        public decimal AmountApplied { get; set; }
    }
}
