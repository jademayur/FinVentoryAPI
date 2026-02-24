namespace FinVentoryAPI.DTOs.CompanyDTOs
{
    public class CompanyResponseDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string GSTNumber { get; set; }
        public string PANNumber { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
    }
}
