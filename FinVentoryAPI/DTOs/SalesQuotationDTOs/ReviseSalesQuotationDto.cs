namespace FinVentoryAPI.DTOs.SalesQuotationDTOs
{
    public class ReviseSalesQuotationDto
    {
        // ── Header ────────────────────────────────────────
        public DateTime QuotationDate { get; set; }
        public DateTime? ValidUntilDate { get; set; }
        public int BusinessPartnerId { get; set; }
        public int LocationId { get; set; }
        public decimal RoundOff { get; set; }
        public string? Remarks { get; set; }
        public int? SalesStateCode { get; set; }
        public int? BillStateCode { get; set; }
        public int? ContactPersonId { get; set; }
        public int? SalesPersonId { get; set; }
        public int? BillAddressId { get; set; }
        public int? ShipAddressId { get; set; }

        // ── Detail lines — user can edit qty/rate, add/remove items ──
        public List<CreateSalesQuotationDetailDto> Details { get; set; } = new();
    }
}
