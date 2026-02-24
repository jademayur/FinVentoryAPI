namespace FinVentoryAPI.DTOs.UserCompany
{
    public class UserCompanyUpdateDto
    {
        public int UserCompanyId { get; set; }
        public int RoleId { get; set; }
        public int CompanyId { get; set; }
        public int FinancialYearId { get; set; }
        public bool IsActive { get; set; }
    }
}
