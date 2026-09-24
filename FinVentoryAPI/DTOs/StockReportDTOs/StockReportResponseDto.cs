namespace FinVentoryAPI.DTOs.StockReportDTOs
{
    public class StockReportResponseDto
    {
        public string ReportType { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public StockReportMetaDto Meta { get; set; }
        public object Data { get; set; }
    }
}
