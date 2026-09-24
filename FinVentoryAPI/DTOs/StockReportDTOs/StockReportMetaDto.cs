namespace FinVentoryAPI.DTOs.StockReportDTOs
{
    public class StockReportMetaDto
    {
        public int TotalRecords { get; set; }
        public decimal TotalValue { get; set; }
        public decimal TotalQty { get; set; }

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalPages { get; set; }
    }
}
