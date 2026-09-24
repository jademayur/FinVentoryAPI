namespace FinVentoryAPI.DTOs.OutstandingReportDTOs
{
    public class OutstandingReportRequestDto
    {
        public string ReportType { get; set; } = "Overall";
        // "Overall" | "BillWise" | "Aging"

        public string PartyType { get; set; } = "Customer";
        // "Customer" | "Supplier"

        public List<int>? BusinessPartnerIds { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
