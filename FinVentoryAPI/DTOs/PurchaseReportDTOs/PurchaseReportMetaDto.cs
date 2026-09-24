namespace FinVentoryAPI.DTOs.PurchaseReportDTOs
{
    public class PurchaseReportMetaDto
    {
        public decimal TotalSubTotal { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalCess { get; set; }
        public decimal TotalNet { get; set; }
        public int TotalRecords { get; set; }

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalPages { get; set; }
    }
}
