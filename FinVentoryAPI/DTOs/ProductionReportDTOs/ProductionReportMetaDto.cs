namespace FinVentoryAPI.DTOs.ProductionReportDTOs
{
    public class ProductionReportMetaDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalPlannedQty { get; set; }
        public decimal TotalActualQty { get; set; }
        public int TotalCompleted { get; set; }
        public int TotalInProgress { get; set; }
        public int TotalDraft { get; set; }
        public int TotalCancelled { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
    }
}
