namespace FinVentoryAPI.DTOs.UserCompany
{
    public class UserCompanyBulkCreateDto
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public List<CompanyYearAssignmentDto> Assignments { get; set; } = new();
    }
}
