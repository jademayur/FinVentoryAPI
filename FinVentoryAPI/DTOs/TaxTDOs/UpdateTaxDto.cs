using FinVentoryAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.TaxTDOs
{
    public class UpdateTaxDto
    {
        public int TaxId { get; set; }
        [Required(ErrorMessage = "Tax name is required.")]
        public string TaxName { get; set; } = string.Empty;
        public string? TaxType { get; set; }
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Only numbers are allowed.")]
        public decimal? TaxRate { get; set; }
        [Required(ErrorMessage = "IGST Rate is required.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Only numbers are allowed.")]
        public decimal IGST { get; set; }
        [Required(ErrorMessage = "SGST Rate is required.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Only numbers are allowed.")]
        public decimal SGST { get; set; }
        [Required(ErrorMessage = "CGST Rate is required.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Only numbers are allowed.")]
        public decimal CGST { get; set; }
        public bool IsActive { get; set; }
    }

}
