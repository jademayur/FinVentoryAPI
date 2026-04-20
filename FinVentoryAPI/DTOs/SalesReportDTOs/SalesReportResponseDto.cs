namespace FinVentoryAPI.DTOs.SalesReportDTOs
{
    public class SalesReportResponseDto
    {
        public string ReportType { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public SalesReportMetaDto Meta { get; set; }
        public object Data { get; set; } // typed per report below
    }
}
