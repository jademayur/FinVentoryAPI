using FinVentoryAPI.Enums;

namespace FinVentoryAPI.Entities
{
    public class BusinessPartner : BaseEntity
    {
        public int BusinessPartnerId { get; set; }
        public int CompanyId { get; set; }
        public string BusinessPartnerCode { get; set; } = string.Empty;   // Auto generated
        public string BusinessPartnerName { get; set; } = string.Empty;
        public string PrintName { get; set; } = string.Empty;
        public BusinessPartnerType Type { get; set; }
        public string Mobile { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; } = 0;
        public int CreditDays { get; set; } = 0;
        public int AccountGroupId { get; set; }
        public int AccountId { get; set; }
        public string? DefaultPriceType { get; set; } = "MRP";
        public ICollection<BusinessPartnerAddress>? BPAddresses { get; set; }
        public ICollection<BusinessPartnerContact>? BPContacts   { get; set; }
        public AccountGroup? accountGroup { get; set; }

    }
}
