namespace FinVentoryAPI.DTOs.OpeningItemBalanceDTOs
{
    public class OpeningSerialResponseDto
    {
        public int SerialId { get; set; }
        public string SerialNo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? WarrantyExpiry { get; set; }
    }
}
