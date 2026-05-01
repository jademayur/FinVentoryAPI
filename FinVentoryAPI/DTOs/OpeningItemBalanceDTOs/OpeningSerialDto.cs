namespace FinVentoryAPI.DTOs.OpeningItemBalanceDTOs
{
    public class OpeningSerialDto
    {
        public int? SerialId { get; set; }
        public string? SerialNo { get; set; }
        public int Status { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? WarrantyExpiry { get; set; }
    }
}
