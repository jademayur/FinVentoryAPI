namespace FinVentoryAPI.DTOs.SalesReturnDTOs
{
    public class UpdateSalesReturnMainDto
    {
        public DateTime ReturnDate { get; set; }
        public int? OriginalInvoiceId { get; set; }
        public string? OriginalInvoiceNo { get; set; }
        public DateTime? OriginalInvoiceDate { get; set; }
        public string NoteType { get; set; } = "Credit";
        public int? SourceInvoiceId { get; set; }
        public int BusinessPartnerId { get; set; }
        public int LocationId { get; set; }
        public int SalesAccountId { get; set; }
        public int? SalesStateCode { get; set; }
        public int? BillStateCode { get; set; }
        public int? BillAddressId { get; set; }
        public decimal RoundOff { get; set; }
        public string? Remarks { get; set; }
        public List<UpdateSalesReturnDetailDto> Details { get; set; } = new();
    }
}
