using System.Text.Json.Serialization;

namespace FinVentoryAPI.DTOs.Gstr1DTOs
{
    public class Gstr1ResponseDto
    {
        public string TaxPeriod { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string GSTIN { get; set; } = string.Empty;

        /// <summary>4 — B2B Taxable Supplies (registered recipients)</summary>
        public List<Gstr1B2BRowDto> B2BSupplies { get; set; } = new();

        /// <summary>5 — B2CL (inter-state, unregistered, > ₹2.5L)</summary>
        public List<Gstr1B2CLRowDto> B2CLSupplies { get; set; } = new();

        /// <summary>7 — B2CS (small / intra-state unregistered) — state-wise</summary>
        public List<Gstr1B2CSRowDto> B2CSSupplies { get; set; } = new();

        /// <summary>6 — Credit / Debit Notes to registered buyers (CDNR)</summary>
        public List<Gstr1CdnRowDto> CDNRSupplies { get; set; } = new();

        /// <summary>6A — Credit / Debit Notes to unregistered buyers (CDNUR)</summary>
        public List<Gstr1CdnRowDto> CDNURSupplies { get; set; } = new();

        /// <summary>9 — Nil / Exempt / Non-GST outward supplies (fixed: all 3 buckets)</summary>
        public Gstr1NilExemptDto NilExempt { get; set; } = new();

        /// <summary>12 — HSN-wise summary of outward supplies</summary>
        public List<Gstr1HsnRowDto> HsnSummary { get; set; } = new();

        /// <summary>13 — Documents issued (invoice series count)</summary>
        public List<Gstr1DocumentIssuedDto> DocumentsIssued { get; set; } = new();

        /// <summary>3.1 — Exports (zero-rated with / without IGST)</summary>
        public List<Gstr1ExportRowDto> Exports { get; set; } = new();

        /// <summary>Summary totals</summary>
        public Gstr1TotalsDto Totals { get; set; } = new();
    }
}
