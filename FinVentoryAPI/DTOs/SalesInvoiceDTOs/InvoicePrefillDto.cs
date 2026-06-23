namespace FinVentoryAPI.DTOs.SalesInvoiceDTOs
{
    public class InvoicePrefillDto
    {
        public int BusinessPartnerId { get; set; }
        public int LocationId { get; set; }
        public int? ContactPersonId { get; set; }
        public int? SalesPersonId { get; set; }
        public int? BillAddressId { get; set; }
        public int? ShipAddressId { get; set; }
        public int? SalesStateCode { get; set; }
        public int? BillStateCode { get; set; }

        public List<InvoicePrefillDetailDto> Details { get; set; } = new();
    }
}
