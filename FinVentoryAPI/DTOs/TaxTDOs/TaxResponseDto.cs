using FinVentoryAPI.Enums;

namespace FinVentoryAPI.DTOs.TaxTDOs
{
    public class TaxResponseDto
    {
        public int TaxId { get; set; }
        public string TaxName { get; set; } = string.Empty;
        public string? TaxType { get; set; }
        public decimal? TaxRate { get; set; }
        public decimal IGST { get; set; }
        public decimal SGST { get; set; }
        public decimal CGST { get; set; }
        public bool  IsActive { get; set; }
        public int? IGSTPostingAccountId { get; set; }
        public int? CGSTPostingAccountId { get; set; }
        public int? SGSTPostingAccountId { get; set; }    

        public string? IGSTPostingAccountName { get; set; }
        public string? CGSTPostingAccountName { get; set; }
        public string? SGSTPostingAccountName { get; set; }


    }
}
