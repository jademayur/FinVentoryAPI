namespace FinVentoryAPI.DTOs.Gstr1DTOs
{
    public class Gstr1HsnSummaryDto
    {
        public string TaxPeriod { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string GSTIN { get; set; } = string.Empty;
        public List<Gstr1HsnRowDto> HsnRows { get; set; } = new();

        // ── Totals ─────────────────────────────────────────────────────
        public decimal TotalTaxableValue => HsnRows.Sum(x => x.TaxableValue);
        public decimal TotalIGST => HsnRows.Sum(x => x.IGSTAmount);
        public decimal TotalCGST => HsnRows.Sum(x => x.CGSTAmount);
        public decimal TotalSGST => HsnRows.Sum(x => x.SGSTAmount);
        public decimal TotalCess => HsnRows.Sum(x => x.CessAmount);
    }
}
