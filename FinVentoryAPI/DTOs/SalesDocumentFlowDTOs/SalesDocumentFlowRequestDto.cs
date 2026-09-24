namespace FinVentoryAPI.DTOs.SalesPipelineDTOs
{
    public class SalesDocumentFlowRequestDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? BusinessPartnerId { get; set; }
        public string? CurrentStage { get; set; } // "Quotation","Order","Delivery","Invoice","Paid"
        public string? QuotationStatus { get; set; }
        public string? SortBy { get; set; } = "QuotationDate";
        public string? SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
