namespace FinVentoryAPI.DTOs.PurchaseInvoiceDTOs
{
    public class PurchaseInvoiceDetailSerialResponseDto
    {
        public int Id { get; set; }
        public int DetailId { get; set; }
        public int SerialId { get; set; }
        public string? SerialNo { get; set; }
        public string? Status { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? WarrantyExpiry { get; set; }
    }
}
