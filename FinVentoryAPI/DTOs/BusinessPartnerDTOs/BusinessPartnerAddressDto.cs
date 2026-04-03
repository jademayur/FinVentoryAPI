using FinVentoryAPI.Enums;

namespace FinVentoryAPI.DTOs.BusinessPartnerDTOs
{
    public class BusinessPartnerAddressDto
    {
        public AddressType Type { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public GstState? State { get; set; }
        public string? StateName { get; set; }   // 👈 add
        public string? StateCode { get; set; }   // 👈 add
        public string? Country { get; set; }
        public string? Pincode { get; set; }
        public GSTType GSTType { get; set; }
        public string? GSTNo { get; set; }
        public bool IsDefault { get; set; }
    }
}
