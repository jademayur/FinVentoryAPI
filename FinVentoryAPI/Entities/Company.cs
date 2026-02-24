using System.ComponentModel.DataAnnotations;

namespace FinVentoryAPI.Entities
{
    public class Company
    {
        [Key]
        public int CompanyId { get; set; }

        [Required]
        [MaxLength(150)]
        public string CompanyName { get; set; }

        [MaxLength(20)]
        public string GSTNumber { get; set; }

        [MaxLength(20)]
        public string PANNumber { get; set; }
        [MaxLength(200)]
        public string Address { get; set; }
        [MaxLength(50)]
        public string City { get; set; }
        [MaxLength(50)]
        public string State { get; set; }
        [MaxLength(10)]
        public string PinCode { get; set; }

        [MaxLength(15)]
        public string Phone { get; set; }
        [MaxLength(15)]
        public string Mobile { get; set; }

        [MaxLength(150)]
        public string Email { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }

        public ICollection<UserCompany> UserCompanies { get; set; }
    }
}
