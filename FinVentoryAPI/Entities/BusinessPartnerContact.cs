namespace FinVentoryAPI.Entities
{
    public class BusinessPartnerContact
    {
        public int Id { get; set; }
        public int BusinessPartnerId { get; set; }

        public string Name { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Designation { get; set; }

        public bool IsPrimary { get; set; }
    }
}
