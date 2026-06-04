namespace FinVentoryAPI.DTOs.SalesQuotationDTOs
{
    public class CopySalesQuotationDto
    {
        public DateTime? QuotationDate { get; set; }
        public DateTime? ValidUntilDate { get; set; }

        /// <summary>Override the customer. If changed, ContactPersonId/BillAddressId/ShipAddressId are reset to null.</summary>
        public int? BusinessPartnerId { get; set; }

        /// <summary>Override the location.</summary>
        public int? LocationId { get; set; }

        public string? Remarks { get; set; }
    }
}
