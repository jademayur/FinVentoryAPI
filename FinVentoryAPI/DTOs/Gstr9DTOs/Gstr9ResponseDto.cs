using FinVentoryAPI.DTOs.Gstr1DTOs;

namespace FinVentoryAPI.DTOs.Gstr9DTOs
{
    public class Gstr9ResponseDto
    {
        public string FinancialYear { get; set; } = string.Empty;   // e.g. "2024-25"
        public string CompanyName { get; set; } = string.Empty;
        public string GSTIN { get; set; } = string.Empty;

        /// <summary>Part II — 4 & 5 : Outward and inward supplies declared</summary>
        public Gstr9Part2Dto OutwardInwardSupplies { get; set; } = new();

        /// <summary>Part III — 6 : ITC availed during the year</summary>
        public Gstr9Part3Dto ITCAvailed { get; set; } = new();

        /// <summary>Part IV — 9 : Tax paid as declared in GSTR-3B returns</summary>
        public Gstr9Part4Dto TaxPaid { get; set; } = new();

        /// <summary>Part V — 10 to 14 : Transactions for previous FY declared in current FY</summary>
        public Gstr9Part5Dto PreviousFYTransactions { get; set; } = new();

        /// <summary>Summary totals</summary>
        public Gstr9TotalsDto Totals { get; set; } = new();

        /// <summary>Month-wise outward supply breakdown (drill-down)</summary>
        public List<Gstr9MonthlyBreakdownDto> MonthlyBreakdown { get; set; } = new();
    }
}
