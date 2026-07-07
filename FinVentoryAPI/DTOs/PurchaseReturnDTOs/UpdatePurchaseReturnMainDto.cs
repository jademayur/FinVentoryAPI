namespace FinVentoryAPI.DTOs.PurchaseReturnDTOs
{
    public class UpdatePurchaseReturnMainDto
    {
        public DateTime ReturnDate { get; set; }
        public int? OriginalInvoiceId { get; set; }
        public string? OriginalInvoiceNo { get; set; }
        public DateTime? OriginalInvoiceDate { get; set; }
        public string NoteType { get; set; } = "Debit";
        public int BusinessPartnerId { get; set; }
        public int LocationId { get; set; }
        public int PurchaseAccountId { get; set; }
        public int? PurchaseStateCode { get; set; }
        public int? BillStateCode { get; set; }
        public int? BillAddressId { get; set; }
        public decimal RoundOff { get; set; }
        public string? Remarks { get; set; }
        public List<UpdatePurchaseReturnDetailDto> Details { get; set; } = new();
    }
}
