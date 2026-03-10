using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.AccountDTOs
{
    public class UpdateAccountGroupDto
    {
        public int AccountId { get; set; }
        [Required(ErrorMessage = "Account name is required.")]
        public string AccountName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Account Group is required.")]
        public int AccountGroupId { get; set; }
        public string? AccountCode { get; set; }
        [Required(ErrorMessage = "Account Type is required.")]
        public Enums.AccountType AccountType { get; set; }
        public Enums.BookType? BookType { get; set; }
        public Enums.BookSubType? BookSubType { get; set; }
        public bool IsActive { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
