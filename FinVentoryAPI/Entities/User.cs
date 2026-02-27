namespace FinVentoryAPI.Entities
{
    public class User
    {
        public int UserId { get; set; }

        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public string? Mobile { get; set; }

        // Platform Level Admin
        public bool IsPlatformAdmin { get; set; } = false;
        public int? RoleId { get; set; } 
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }

        // Navigation
       // public ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();
        public virtual Role Role { get; set; } = null!;
    }
}
