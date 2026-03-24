using System.ComponentModel.DataAnnotations.Schema;

namespace FinVentoryAPI.Entities
{
    public class Tax : BaseEntity
    {
        public int TaxId { get; set; }
        public int CompanyId { get; set; }
        public string TaxName { get; set; } = string.Empty;
        public string? TaxType { get; set; }
        public decimal? TaxRate { get; set; }
        public decimal IGST { get; set; }
        public decimal SGST { get; set; }
        public decimal CGST { get; set; }
        public int? IGSTPostingAccountId { get; set; }
        public int? CGSTPostingAccountId { get; set; }
        public int? SGSTPostingAccountId { get; set; }

        [ForeignKey(nameof(IGSTPostingAccountId))]
        public Account? IGSTAccount { get; set; }

        [ForeignKey(nameof(CGSTPostingAccountId))]
        public Account? CGSTAccount { get; set; }

        [ForeignKey(nameof(SGSTPostingAccountId))]
        public Account? SGSTAccount { get; set; }


    }
}
