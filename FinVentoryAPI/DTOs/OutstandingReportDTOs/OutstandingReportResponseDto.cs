namespace FinVentoryAPI.DTOs.OutstandingReportDTOs
{
    public class OutstandingReportResponseDto
    {
        public string ReportType { get; set; } = "";
        public string PartyType { get; set; } = "";
        public OutstandingReportMetaDto Meta { get; set; } = new();
        public object Data { get; set; } = new();
    }
}
