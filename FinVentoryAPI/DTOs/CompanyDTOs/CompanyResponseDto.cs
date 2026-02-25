using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.CompanyDTOs
{
    public class CompanyResponseDto
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string GSTNumber { get; set; }
        public string PANNumber { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }

        public string PinCode { get; set; }
        public string Phone { get; set; }        
        public string Mobile { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
    }
}
