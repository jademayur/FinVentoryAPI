namespace FinVentoryAPI.DTOs.Gstr3bDTOs
{
    public class Gstr3bResponseDto
    {
        public string TaxPeriod { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string GSTIN { get; set; } = string.Empty;

        /// <summary>3.1 — Details of Outward Supplies and Inward Supplies liable to reverse charge</summary>
        public Section31Dto OutwardSupplies { get; set; } = new();

        /// <summary>3.2 — Inter-state supplies to unregistered persons / composition dealers / UIN holders</summary>
        public Section32Dto InterStateSupplies { get; set; } = new();

        /// <summary>4 — Eligible ITC</summary>
        public Section4Dto EligibleITC { get; set; } = new();

        /// <summary>5 — Values of exempt, nil-rated and non-GST inward supplies</summary>
        public Section5Dto ExemptSupplies { get; set; } = new();

        /// <summary>5.1 — Interest and Late Fee</summary>
        public Section51Dto InterestLateFee { get; set; } = new();

        /// <summary>Computed tax payable summary</summary>
        public TaxPayableSummaryDto TaxPayable { get; set; } = new();
    }
}
