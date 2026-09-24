namespace FinVentoryAPI.DTOs.SalesPipelineDTOs
{
    public class SalesDocumentFlowResponseDto
    {
        public List<SalesDocumentFlowRowDto> Data { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }

        // Summary
        public decimal TotalQuotationAmount { get; set; }
        public decimal TotalOrderAmount { get; set; }
        public decimal TotalDeliveryAmount { get; set; }
        public decimal TotalInvoiceAmount { get; set; }
        public decimal TotalPaymentAmount { get; set; }
        public decimal TotalOutstanding { get; set; }
    }
}
