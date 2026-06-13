namespace FinVentoryAPI.DTOs.UserCompany
{
    public class CompanyYearAssignmentDto
    {
        public int CompanyId { get; set; }
        public List<int> FinancialYearIds { get; set; } = new();
    }
}
