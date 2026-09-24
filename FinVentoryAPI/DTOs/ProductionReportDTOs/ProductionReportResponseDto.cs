namespace FinVentoryAPI.DTOs.ProductionReportDTOs
{
    public class ProductionReportResponseDto
    {
        public string ReportType { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public ProductionReportMetaDto Meta { get; set; }
        public object Data { get; set; }
    }
}
