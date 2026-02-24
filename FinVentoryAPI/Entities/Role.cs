namespace FinVentoryAPI.Entities
{
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<UserCompany> UserCompanies { get; set; }
    }
}
