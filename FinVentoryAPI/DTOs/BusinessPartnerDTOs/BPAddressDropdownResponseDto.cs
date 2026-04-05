namespace FinVentoryAPI.DTOs.BusinessPartnerDTOs
{
    public class BPAddressDropdownResponseDto
    {
        public BPAddressResponseDto? DefaultBillAddress { get; set; }
        public BPAddressResponseDto? DefaultShipAddress { get; set; }
        public BusinessPartnerContactResponseDto? DefaultContact { get; set; }
        public List<BPAddressResponseDto> BillAddresses { get; set; } = new();
        public List<BPAddressResponseDto> ShipAddresses { get; set; } = new();
        public List<BusinessPartnerContactResponseDto> Contacts { get; set; } = new();
    }
}
