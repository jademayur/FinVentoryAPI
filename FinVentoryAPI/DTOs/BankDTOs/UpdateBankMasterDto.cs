using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.BankDTOs
{
    public class UpdateBankMasterDto
    {
        public int BankId { get; set; }

        [Required(ErrorMessage = "Bank name is required.")]
        [MaxLength(100, ErrorMessage = "Bank name cannot exceed 100 characters.")]
        public string BankName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Branch is required.")]
        [MaxLength(100, ErrorMessage = "Branch cannot exceed 100 characters.")]
        public string Branch { get; set; } = string.Empty;

        [Required(ErrorMessage = "Account number is required.")]
        [MaxLength(20, ErrorMessage = "Account number cannot exceed 20 characters.")]
        public string AccountNo { get; set; } = string.Empty;

        [MaxLength(11, ErrorMessage = "Swift code cannot exceed 11 characters.")]
        public string? SwiftCode { get; set; }

        [Required(ErrorMessage = "IFSC code is required.")]
        [MaxLength(11, ErrorMessage = "IFSC code cannot exceed 11 characters.")]
        public string IFSCCode { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public int ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
