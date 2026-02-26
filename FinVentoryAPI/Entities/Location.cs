namespace FinVentoryAPI.Entities
{
    public class Location
    {
        public int LocationId { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; }

        public string LocationName { get; set; } = string.Empty;
        public string? LocationCode { get; set; }

        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? Pincode { get; set; }

        public bool IsHeadOffice { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
