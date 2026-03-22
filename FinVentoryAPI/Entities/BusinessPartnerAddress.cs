namespace FinVentoryAPI.Entities
{
    public class BusinessPartnerAddress
    {
        public int Id { get; set; }
        public int BusinessPartnerId { get; set; }

        public AddressType Type { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Pincode { get; set; }

        public bool IsDefault { get; set; }
    }
}
