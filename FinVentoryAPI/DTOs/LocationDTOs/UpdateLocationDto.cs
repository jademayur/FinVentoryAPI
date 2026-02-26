namespace FinVentoryAPI.DTOs.LocationDTOs
{
    public class UpdateLocationDTO
    {
        public int LocationId { get; set; }

        public string LocationName { get; set; } = string.Empty;
        public string? LocationCode { get; set; }

        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }

        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? Pincode { get; set; }

        public bool IsHeadOffice { get; set; }
        public bool IsActive { get; set; }

        public int UpdatedBy { get; set; }
    }
}
