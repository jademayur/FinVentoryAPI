namespace FinVentoryAPI.DTOs.PurchaseInvoiceDTOs
{
    public class InvoicePrefillDto
    {
        public int BusinessPartnerId { get; set; }
        public int LocationId { get; set; }
        public int? ContactPersonId { get; set; }
        public int? BillAddressId { get; set; }
        public int? ShipAddressId { get; set; }
        public int? PurchaseStateCode { get; set; }
        public int? BillStateCode { get; set; }
        public List<InvoicePrefillDetailDto> Details { get; set; } = new();
    }
}
