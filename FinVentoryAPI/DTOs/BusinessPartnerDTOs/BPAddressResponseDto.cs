using FinVentoryAPI.Enums;

namespace FinVentoryAPI.DTOs.BusinessPartnerDTOs
{
    public class BPAddressResponseDto
    {
        public int BPAddressId { get; set; }
        public int BusinessPartnerId { get; set; }
        public string? AddressType { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public int? StateCode { get; set; }
        public string? StateName { get; set; }
        public string? Country { get; set; }
        public string? Pincode { get; set; }
        public string? GSTType { get; set; }
        public string? GSTNo { get; set; }
        public bool IsDefault { get; set; }
    }
}
