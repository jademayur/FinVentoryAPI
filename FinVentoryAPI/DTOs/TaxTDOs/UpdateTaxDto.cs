using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.TaxTDOs
{
    public class UpdateTaxDto
    {
        public int TaxId { get; set; }

        [Required(ErrorMessage = "Tax name is required.")]
        public string TaxName { get; set; } = string.Empty;

        public string? TaxType { get; set; }

        public decimal? TaxRate { get; set; }

        [Required(ErrorMessage = "IGST Rate is required.")]
        public decimal IGST { get; set; }

        [Required(ErrorMessage = "SGST Rate is required.")]
        public decimal SGST { get; set; }

        [Required(ErrorMessage = "CGST Rate is required.")]
        public decimal CGST { get; set; }

        public bool IsActive { get; set; }
        public int? IGSTPostingAccountId { get; set; }
        public int? CGSTPostingAccountId { get; set; }
        public int? SGSTPostingAccountId { get; set; }
    }
}