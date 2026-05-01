namespace FinVentoryAPI.DTOs.SalesInvoiceDTOs
{
    public class SerialLookupDto
    {
        public int SerialId { get; set; }
        public string SerialNo { get; set; } = string.Empty;
        public DateTime? WarrantyExpiry { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
