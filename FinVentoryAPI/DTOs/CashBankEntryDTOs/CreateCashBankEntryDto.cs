using FinVentoryAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.CashBankEntryDTOs
{
    public class CreateCashBankEntryDto
    {
        
        public int BookType { get; set; }

        [Required(ErrorMessage = "Entry date is required.")]
        public DateOnly EntryDate { get; set; }

        [Required(ErrorMessage = "Entry type is required.")]
        [EnumDataType(typeof(EntryType), ErrorMessage = "Invalid entry type. Use Receipt or Payment.")]
        public EntryType EntryType { get; set; }

        /// <summary>
        /// Header account — Cash account for CASH BOOK, Bank account for BANK BOOK.
        /// </summary>
        [Required(ErrorMessage = "Head account is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Head account is required.")]
        public int HeadAccountId { get; set; }

        [MaxLength(50, ErrorMessage = "Payment mode cannot exceed 50 characters.")]
        public string? PaymentMode { get; set; }

        /// <summary>Required for BANK BOOK entries — enforced in service layer.</summary>
        [MaxLength(100, ErrorMessage = "Reference no cannot exceed 100 characters.")]
        public string? ReferenceNo { get; set; }

        public DateOnly? ReferenceDate { get; set; }

        [MaxLength(500, ErrorMessage = "Narration cannot exceed 500 characters.")]
        public string? Narration { get; set; }

        [Required(ErrorMessage = "At least one account line is required.")]
        [MinLength(1, ErrorMessage = "At least one account line is required.")]
        public List<CreateCashBankEntryLineDto> Lines { get; set; } = new();
    }
}
