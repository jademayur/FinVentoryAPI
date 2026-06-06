namespace FinVentoryAPI.DTOs.SalesOrderDTOs
{
    public class QuotationPickerDto
    {
        public int QuotationId { get; set; }
        public string QuotationNo { get; set; } = string.Empty;
        public DateTime QuotationDate { get; set; }
        public DateTime? ValidUntilDate { get; set; }
        public decimal NetTotal { get; set; }
        public int? SalesPersonId { get; set; }
        public string? SalesPersonName { get; set; }
    }
}
