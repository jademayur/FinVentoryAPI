using System.Data;

namespace FinVentoryAPI.Entities
{
    public class UserCompany
    {
        public int UserCompanyId { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; }

       // public bool IsActive { get; set; } = true;

      //  public DateTime? AssignedDate { get; set; } = DateTime.UtcNow;
    }
}
