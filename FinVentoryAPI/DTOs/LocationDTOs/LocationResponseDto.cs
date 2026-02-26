namespace FinVentoryAPI.DTOs.LocationDTOs
{
    public class LocationResponseDto
    {
        public int LocationId { get; set; }

        public int CompanyId { get; set; }
       // public string CompanyName { get; set; } = string.Empty;

        public string LocationName { get; set; } = string.Empty;
        public string? LocationCode { get; set; }

        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }

        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? Pincode { get; set; }

        public bool? IsHeadOffice { get; set; } = false;
        public bool? IsActive { get; set; } = true;

        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }

        public string? UpdatedByName { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
