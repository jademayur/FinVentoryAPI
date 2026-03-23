namespace FinVentoryAPI.DTOs.BusinessPartnerDTOs
{
    public class BPContactDto
    {
        public string? Name { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? Designation { get; set; }
        public bool IsPrimary { get; set; } = false;
    }
}
