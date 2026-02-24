namespace FinVentoryAPI.Entities
{
    public class User
    {
        public int UserId { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public string Mobile { get; set; }

        // Platform Level Admin
        public bool IsPlatformAdmin { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<UserCompany> UserCompanies { get; set; }
    }
}
