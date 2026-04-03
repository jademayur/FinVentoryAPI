namespace FinVentoryAPI.DTOs.SalesPersonDTOs
{
    public class SalesPersonResponseDto
    {
        public int SalesPersonId { get; set; }
        public string SalesPersonCode { get; set; }
        public string SalesPersonName { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public decimal? CommissionPct { get; set; }
        public bool IsActive { get; set; }
    }
}
