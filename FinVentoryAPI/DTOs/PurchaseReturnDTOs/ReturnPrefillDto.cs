namespace FinVentoryAPI.DTOs.PurchaseReturnDTOs
{
    public class ReturnPrefillDto
    {
        public int BusinessPartnerId { get; set; }
        public int LocationId { get; set; }
        public int? BillAddressId { get; set; }
        public int? PurchaseStateCode { get; set; }
        public int? BillStateCode { get; set; }
        public int? PurchaseAccountId { get; set; }
        public int OriginalInvoiceId { get; set; }
        public string? OriginalInvoiceNo { get; set; }
        public DateTime? OriginalInvoiceDate { get; set; }
        public List<ReturnPrefillDetailDto> Details { get; set; } = new();
    }
}
