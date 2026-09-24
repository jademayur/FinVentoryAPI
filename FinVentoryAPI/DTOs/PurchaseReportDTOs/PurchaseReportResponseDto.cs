namespace FinVentoryAPI.DTOs.PurchaseReportDTOs
{
    public class PurchaseReportResponseDto
    {
        public string ReportType { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public PurchaseReportMetaDto Meta { get; set; }
        public object Data { get; set; } // typed per report below
    }
}
