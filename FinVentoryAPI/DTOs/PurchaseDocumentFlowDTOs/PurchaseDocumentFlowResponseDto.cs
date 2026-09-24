namespace FinVentoryAPI.DTOs.PurchaseDocumentFlowDTOs
{
    public class PurchaseDocumentFlowResponseDto
    {
        public List<PurchaseDocumentFlowRowDto> Data { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }

        // Summary
        public decimal TotalOrderAmount { get; set; }
        public decimal TotalGrnAmount { get; set; }
        public decimal TotalInvoiceAmount { get; set; }
        public decimal TotalPaymentAmount { get; set; }
        public decimal TotalOutstanding { get; set; }
    }
}
