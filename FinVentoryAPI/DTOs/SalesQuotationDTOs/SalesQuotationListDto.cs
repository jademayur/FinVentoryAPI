namespace FinVentoryAPI.DTOs.SalesQuotationDTOs
{
    public class SalesQuotationListDto
    {
        public int QuotationId { get; set; }
        public string QuotationNo { get; set; } = string.Empty;
        public DateTime QuotationDate { get; set; }
        public DateTime? ValidUntilDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string BusinessPartnerName { get; set; } = string.Empty;
        public decimal NetTotal { get; set; }

        public int? ParentQuotationId { get; set; }
        public int RevisionNo { get; set; }
    }
}
