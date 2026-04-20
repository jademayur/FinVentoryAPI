namespace FinVentoryAPI.DTOs.SalesReportDTOs
{
    public class SalesReportRequestDto
    {
        public string ReportType { get; set; } = "SalesRegister";
        // "SalesRegister" | "SalesRegisterDetails" | "ItemWise"
        // "PartyWise"     | "TaxWise" | "MonthlySummary" | "MonthlyGST"

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // All optional — empty list = no filter
        public List<int>? BusinessPartnerIds { get; set; }
        public List<int>? ItemIds { get; set; }
        public List<int>? SalesPersonIds { get; set; }
        public List<int>? LocationIds { get; set; }
        public List<string>? GstTypes { get; set; } // "B2B","B2C","EXPORT","EXEMPT"
        public List<string>? Statuses { get; set; } // "Draft","Confirmed","Cancelled"
    }
}
