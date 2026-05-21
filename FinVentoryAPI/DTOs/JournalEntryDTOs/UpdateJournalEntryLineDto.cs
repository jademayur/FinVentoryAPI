using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.JournalEntryDTOs
{
    public class UpdateJournalEntryLineDto
    {
        [Required(ErrorMessage = "Account is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Account is required.")]
        public int AccountId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Debit must be 0 or greater.")]
        public decimal Debit { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Credit must be 0 or greater.")]
        public decimal Credit { get; set; }

        [MaxLength(500, ErrorMessage = "Narration cannot exceed 500 characters.")]
        public string? Narration { get; set; }
    }
}
