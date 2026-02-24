namespace FinVentoryAPI.DTOs.UserCompany
{
    public class UserCompanyUpdateDto
    {
        public int UserCompanyId { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; }
    }
}
