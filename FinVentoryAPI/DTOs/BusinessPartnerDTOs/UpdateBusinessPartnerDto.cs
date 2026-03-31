using FinVentoryAPI.Entities;
using FinVentoryAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.DTOs.BusinessPartnerDTOs
{
    public class UpdateBusinessPartnerDto
    {
        public int Id { get; set; }
        public string BPCode { get; set; } = string.Empty;
        [Required(ErrorMessage = "Business Partner Name is required.")]
        public string BPName { get; set; } = string.Empty;
        public string PrintName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Business Partner Name is required.")]
        public BusinessPartnerType BPType { get; set; }
        public string Mobile { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; } = 0;
        public int CreditDays { get; set; } = 0;
        [Required(ErrorMessage = "Group Name is required.")]
        public int AccountGroupId { get; set; }
        public int AccountId { get; set; }
        public string? DefaultPriceType { get; set; } = "MRP";
        public ICollection<BusinessPartnerAddress> BPAddresses { get; set; }
        public ICollection<BusinessPartnerContact>? BPContacts { get; set; }
     
        public bool IsActive { get; set; }
    }
}
