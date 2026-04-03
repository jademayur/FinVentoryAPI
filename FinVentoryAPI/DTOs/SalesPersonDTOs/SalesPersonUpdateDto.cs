using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.SalesPersonDTOs
{
    public class SalesPersonUpdateDto
    {
        [Required(ErrorMessage = "Sales Person Code is required.")]
        [MaxLength(50, ErrorMessage = "Code cannot exceed 50 characters.")]
        public string SalesPersonCode { get; set; }

        [Required(ErrorMessage = "Sales Person Name is required.")]
        [MaxLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
        public string SalesPersonName { get; set; }

        [MaxLength(15, ErrorMessage = "Mobile cannot exceed 15 characters.")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Mobile must be exactly 10 digits.")]
        public string? Mobile { get; set; }

        [MaxLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string? Email { get; set; }

        [Range(0, 100, ErrorMessage = "Commission % must be between 0 and 100.")]
        public decimal? CommissionPct { get; set; }

        public bool IsActive { get; set; }
    }
}
