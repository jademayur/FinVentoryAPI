namespace FinVentoryAPI.DTOs.HsnDTOs
{
    public class HsnResponseDto
    {
        public int HsnId { get; set; }
        public string HsnName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string HsnType { get; set; } = string.Empty;
        public int TaxId { get; set; }
        public string TaxName { get; set; } = string.Empty;
        public decimal? Cess { get; set; }
        public bool IsActive { get; set; }
       
    }
}
