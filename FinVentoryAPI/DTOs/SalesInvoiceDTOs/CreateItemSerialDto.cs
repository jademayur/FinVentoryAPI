namespace FinVentoryAPI.DTOs.SalesInvoiceDTOs
{
    public class CreateItemSerialDto
    {
        public int ItemId { get; set; }
        public string SerialNo { get; set; } = string.Empty;
        public DateTime? PurchaseDate { get; set; }
        public DateTime? WarrantyExpiry { get; set; }
        public string? Remarks { get; set; }
    }
}
