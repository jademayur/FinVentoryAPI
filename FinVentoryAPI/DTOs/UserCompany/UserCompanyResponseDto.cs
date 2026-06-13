namespace FinVentoryAPI.DTOs.UserCompany
{
    public class UserCompanyResponseDto
    {
        public int UserCompanyId { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; }

        public int CompanyId { get; set; }
        public string CompanyName { get; set; }

        public int FinancialYearId { get; set; }
        public string FinancialYearName { get; set; }

        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
