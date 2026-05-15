using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.CashBankEntryDTOs
{
    public class UpdateCashBankEntryLineDto
    {
        /// <summary>0 = new line (insert), greater than 0 = existing line (update).</summary>
        [Range(0, int.MaxValue, ErrorMessage = "CashBankEntryLineId must be 0 or greater.")]
        public int CashBankEntryLineId { get; set; }

        [Required(ErrorMessage = "Account is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Account is required.")]
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, 99999999.99, ErrorMessage = "Amount must be between 0.01 and 9,99,99,999.99.")]
        public decimal Amount { get; set; }

        [MaxLength(200, ErrorMessage = "Narration cannot exceed 200 characters.")]
        public string? Narration { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Sort order must be 0 or greater.")]
        public int SortOrder { get; set; }
    }
}
