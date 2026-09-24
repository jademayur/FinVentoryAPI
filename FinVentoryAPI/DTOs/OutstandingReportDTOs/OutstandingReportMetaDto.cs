namespace FinVentoryAPI.DTOs.OutstandingReportDTOs
{
    public class OutstandingReportMetaDto
    {
        public decimal TotalOutstanding { get; set; }
        public int TotalInvoices { get; set; }
        public decimal TotalBilled { get; set; }
        public decimal TotalPaid { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
    }
}
