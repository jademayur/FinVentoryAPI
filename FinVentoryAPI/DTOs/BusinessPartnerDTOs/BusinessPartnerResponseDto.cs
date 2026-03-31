using FinVentoryAPI.DTOs.ItemDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Enums;

namespace FinVentoryAPI.DTOs.BusinessPartnerDTOs
{
    public class BusinessPartnerResponseDto
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
        public bool IsActive { get; set; }
        public string? DefaultPriceType { get; set; } 
        public List<BusinessPartnerAddressDto>? BPAddresses { get; set; }
        public List<BusinessPartnerContactDto>? BPContacts { get; set; }

    }
}
