namespace FinVentoryAPI.DTOs.PurchaseDocumentFlowDTOs
{
    public class PurchaseDocumentFlowRequestDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? BusinessPartnerId { get; set; }
        public string? CurrentStage { get; set; }
        public string? OrderStatus { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
