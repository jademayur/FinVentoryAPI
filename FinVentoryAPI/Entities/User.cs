namespace FinVentoryAPI.Entities
{
    public class User : BaseEntity
    {
        public int UserId { get; set; }

        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public string? Mobile { get; set; }

        public int RoleId { get; set; }

        // Navigation
        public Role Role { get; set; }
    }
}
