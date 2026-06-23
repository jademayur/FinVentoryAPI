namespace FinVentoryAPI.DTOs.Gstr1DTOs
{
    public class Gstr1DocSeriesSummaryDto
    {
        public string TaxPeriod { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string GSTIN { get; set; } = string.Empty;

        /// <summary>One row per document nature</summary>
        public List<Gstr1DocSeriesRowDto> DocumentSeries { get; set; } = new();

        // ── Totals ─────────────────────────────────────────────────────
        public int TotalDocumentsIssued => DocumentSeries.Sum(x => x.TotalIssued);
        public int TotalDocumentsCancelled => DocumentSeries.Sum(x => x.TotalCancelled);
        public int TotalNetDocuments => DocumentSeries.Sum(x => x.NetIssued);
    }
}
