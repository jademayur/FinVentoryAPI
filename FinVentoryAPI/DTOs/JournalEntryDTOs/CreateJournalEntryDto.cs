using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.JournalEntryDTOs
{
    public class CreateJournalEntryDto
    {
        [Required(ErrorMessage = "Account is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Account is required.")]
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Entry date is required.")]
        public DateOnly EntryDate { get; set; }

        [MaxLength(50, ErrorMessage = "Entry no cannot exceed 50 characters.")]
        public string? EntryNo { get; set; }

        [MaxLength(500, ErrorMessage = "Narration cannot exceed 500 characters.")]
        public string? Narration { get; set; }

        [Required(ErrorMessage = "At least one line is required.")]
        [MinLength(1, ErrorMessage = "At least one line is required.")]
        public List<CreateJournalEntryLineDto> Lines { get; set; } = new();
    }
}
