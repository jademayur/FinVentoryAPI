namespace FinVentoryAPI.Entities
{
    public class BusinessPartner : BaseEntity
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }

        public string Code { get; set; }   // Auto generated
        public string Name { get; set; }

        public BusinessPartnerType Type { get; set; }

        public string Mobile { get; set; }
        public string Email { get; set; }
    }
}
