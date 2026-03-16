using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.TaxTDOs
{
    public class CreateTaxDto
    {
        [Required(ErrorMessage = "Tax name is required.")]
        public string TaxName { get; set; } = string.Empty;       
        public string? TaxType { get; set; }
        [RegularExpression(@"^\d+(\.\d{1,4})?$", ErrorMessage = "Only valid numbers are allowed (e.g. 9 or 9.5).")]
        public decimal? TaxRate { get; set; }
        [Required(ErrorMessage = "IGST Rate is required.")]
        [RegularExpression(@"^\d+(\.\d{1,4})?$", ErrorMessage = "Only valid numbers are allowed (e.g. 9 or 9.5).")]
        public decimal IGST { get; set; }
        [Required(ErrorMessage = "SGST Rate is required.")]
        [RegularExpression(@"^\d+(\.\d{1,4})?$", ErrorMessage = "Only valid numbers are allowed (e.g. 9 or 9.5).")]
        public decimal SGST { get; set; }
        [Required(ErrorMessage = "CGST Rate is required.")]
        [RegularExpression(@"^\d+(\.\d{1,4})?$", ErrorMessage = "Only valid numbers are allowed (e.g. 9 or 9.5).")]
        public decimal CGST { get; set; }
    }
}
